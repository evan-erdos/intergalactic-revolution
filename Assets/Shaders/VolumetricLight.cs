//  Copyright(c) 2016, Michal Skalsky
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification,
//  are permitted provided that the following conditions are met:
//
//  1. Redistributions of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//
//  2. Redistributions in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the documentation
//     and/or other materials provided with the distribution.
//
//  3. Neither the name of the copyright holder nor the names of its contributors
//     may be used to endorse or promote products derived from this software without
//     specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
//  EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
//  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT
//  SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
//  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
//  OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
//  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
//  TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
//  EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.



using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using System;

[RequireComponent(typeof(Light))]
public class VolumetricLight : MonoBehaviour {
    bool _reversedZ = false;
    Light _light;
    Material _material;
    CommandBuffer _commandBuffer;
    CommandBuffer _cascadeShadowCommandBuffer;
    Vector4[] _frustumCorners = new Vector4[4];

    [Range(1, 64)] public int SampleCount = 8;
    [Range(0.0f, 1.0f)] public float ScatteringCoef = 0.5f;
    [Range(0.0f, 0.1f)] public float ExtinctionCoef = 0.01f;
    [Range(0.0f, 1.0f)] public float SkyboxExtinctionCoef = 0.9f;
    [Range(0.0f, 0.999f)] public float MieG = 0.1f;
    public bool HeightFog = false;
    [Range(0, 0.5f)] public float HeightScale = 0.1f;
    public float GroundLevel = 0;
    public bool Noise = false;
    public float NoiseScale = 0.015f;
    public float NoiseIntensity = 1;
    public float NoiseIntensityOffset = 0.3f;
    public Vector2 NoiseVelocity = new Vector2(3, 3);
    [Tooltip("")] public float MaxRayLength = 400;
    public Vector3 Displacement { get { return _light.transform.position-Camera.current.transform.position; } }
    public Vector4 HeightVector { get { return new Vector4(GroundLevel, HeightScale); } }
    public Light Light { get { return _light; } }
    public Material VolumetricMaterial { get { return _material; } }

    public event Action<VolumetricLightRenderer,VolumetricLight,CommandBuffer,Matrix4x4> CustomRenderEvent;

    void Start() {
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11
            || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12
            || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal
            || SystemInfo.graphicsDeviceType == GraphicsDeviceType.PlayStation4
            || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan
            || SystemInfo.graphicsDeviceType == GraphicsDeviceType.XboxOne) _reversedZ = true;
        _commandBuffer = new CommandBuffer();
        _commandBuffer.name = "Light Command Buffer";
        _cascadeShadowCommandBuffer = new CommandBuffer();
        _cascadeShadowCommandBuffer.name = "Dir Light Command Buffer";
        _cascadeShadowCommandBuffer.SetGlobalTexture("_CascadeShadowMapTexture", new UnityEngine.Rendering.RenderTargetIdentifier(UnityEngine.Rendering.BuiltinRenderTextureType.CurrentActive));

        _light = GetComponent<Light>(); // _light.RemoveAllCommandBuffers();
        if(_light.type == LightType.Directional) {
            _light.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, _commandBuffer);
            _light.AddCommandBuffer(LightEvent.AfterShadowMap, _cascadeShadowCommandBuffer);
        } else _light.AddCommandBuffer(LightEvent.AfterShadowMap, _commandBuffer);

        var shader = Shader.Find("Sandbox/VolumetricLight");
        if (shader==null) throw new Exception("Critical Error: 'Sandbox/VolumetricLight' shader is missing.");
        _material = new Material(shader); // new Material(VolumetricLightRenderer.GetLightMaterial());
    }

    void OnEnable() { VolumetricLightRenderer.PreRenderEvent += VolumetricLightRenderer_PreRenderEvent; }
    void OnDisable() { VolumetricLightRenderer.PreRenderEvent -= VolumetricLightRenderer_PreRenderEvent; }
    public void OnDestroy() { Destroy(_material); }

    void VolumetricLightRenderer_PreRenderEvent(VolumetricLightRenderer renderer, Matrix4x4 viewProj) {
        if (_light==null || _light.gameObject==null) VolumetricLightRenderer.PreRenderEvent -= VolumetricLightRenderer_PreRenderEvent;
        if (!_light.gameObject.activeInHierarchy || _light.enabled == false) return;

        _material.SetVector("_CameraForward", Camera.current.transform.forward);
        _material.SetInt("_SampleCount", SampleCount);
        _material.SetVector("_NoiseVelocity", new Vector4(NoiseVelocity.x, NoiseVelocity.y)*NoiseScale);
        _material.SetVector("_NoiseData", new Vector4(NoiseScale, NoiseIntensity, NoiseIntensityOffset));
        _material.SetVector("_MieG", new Vector4(1-(MieG*MieG), 1+(MieG*MieG), 2*MieG, 1f/(4f*Mathf.PI)));
        _material.SetVector("_VolumetricLight", new Vector4(ScatteringCoef, ExtinctionCoef, _light.range, 1-SkyboxExtinctionCoef));
        _material.SetTexture("_CameraDepthTexture", renderer.GetVolumeLightDepthBuffer());
        _material.SetFloat("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);

        if (_light.type == LightType.Point) SetupPointLight(renderer, viewProj);
        else if(_light.type == LightType.Spot) SetupSpotLight(renderer, viewProj);
        else if (_light.type == LightType.Directional) SetupDirectionalLight(renderer, viewProj);
        if (HeightFog) { _material.EnableKeyword("HEIGHT_FOG"); _material.SetVector("_HeightFog", HeightVector); }
        else _material.DisableKeyword("HEIGHT_FOG");
    }

    void Update() { _commandBuffer.Clear(); }

    void SetupPointLight(VolumetricLightRenderer renderer, Matrix4x4 viewProj) {
        var pass = 0;
        if (!IsCameraInPointLightBounds()) pass = 2;
        _material.SetPass(pass);
        var mesh = VolumetricLightRenderer.GetPointLightMesh();
        var scale = _light.range*2;
        var world = Matrix4x4.TRS(transform.position, _light.transform.rotation, new Vector3(scale, scale, scale));
        var lpos = _light.transform.position;
        _material.SetMatrix("_WorldViewProj", viewProj * world);
        _material.SetMatrix("_WorldView", Camera.current.worldToCameraMatrix * world);

        if (Noise) _material.EnableKeyword("NOISE");
        else _material.DisableKeyword("NOISE");

        _material.SetVector("_LightPos", new Vector4(lpos.x, lpos.y, lpos.z, 1f/(_light.range*_light.range)));
        _material.SetColor("_LightColor", _light.color * _light.intensity);

        if (_light.cookie != null) {
            var view = Matrix4x4.TRS(_light.transform.position, _light.transform.rotation, Vector3.one).inverse;
            _material.SetMatrix("_MyLightMatrix0", view);
            _material.EnableKeyword("POINT_COOKIE");
            _material.DisableKeyword("POINT");
            _material.SetTexture("_LightTexture0", _light.cookie);
        } else { _material.EnableKeyword("POINT"); _material.DisableKeyword("POINT_COOKIE"); }

        var forceShadowsOff = false;
        if (Displacement.magnitude >= QualitySettings.shadowDistance) forceShadowsOff = true;

        if (_light.shadows != LightShadows.None && forceShadowsOff==false) {
            _material.EnableKeyword("SHADOWS_CUBE");
            _commandBuffer.SetGlobalTexture("_ShadowMapTexture", BuiltinRenderTextureType.CurrentActive);
            _commandBuffer.SetRenderTarget(renderer.GetVolumeLightBuffer());
            _commandBuffer.DrawMesh(mesh, world, _material, 0, pass);
            if (CustomRenderEvent != null) CustomRenderEvent(renderer, this, _commandBuffer, viewProj);
        } else {
            _material.DisableKeyword("SHADOWS_CUBE");
            renderer.GlobalCommandBuffer.DrawMesh(mesh, world, _material, 0, pass);
            if (CustomRenderEvent != null) CustomRenderEvent(renderer, this, renderer.GlobalCommandBuffer, viewProj);
        }
    }


    void SetupSpotLight(VolumetricLightRenderer renderer, Matrix4x4 viewProj) {
        var pass = IsCameraInSpotLightBounds()?3:1;
        var mesh = VolumetricLightRenderer.GetSpotLightMesh();
        var scale = _light.range;
        var angleScale = Mathf.Tan((_light.spotAngle + 1) * 0.5f * Mathf.Deg2Rad) * _light.range;
        var world = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(angleScale, angleScale, scale));
        var view = Matrix4x4.TRS(_light.transform.position, _light.transform.rotation, Vector3.one).inverse;
        var clip = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0.0f), Quaternion.identity, new Vector3(-0.5f, -0.5f, 1.0f));
        var proj = Matrix4x4.Perspective(_light.spotAngle, 1, 0, 1);
        var lpos = _light.transform.position;
        _material.SetMatrix("_MyLightMatrix0", clip * proj * view);
        _material.SetMatrix("_WorldViewProj", viewProj * world);
        _material.SetVector("_LightPos", new Vector4(lpos.x, lpos.y, lpos.z, 1.0f / (_light.range * _light.range)));
        _material.SetVector("_LightColor", _light.color * _light.intensity);
        var apex = transform.position;
        var axis = transform.forward;
        var center = apex + axis * _light.range; // plane equation ax + by + cz + d = 0
        var d = -Vector3.Dot(center, axis); // precompute d here to lighten the shader
        _material.SetFloat("_PlaneD", d);
        _material.SetFloat("_CosAngle", Mathf.Cos((_light.spotAngle+1) * 0.5f * Mathf.Deg2Rad));
        _material.SetVector("_ConeApex", new Vector4(apex.x, apex.y, apex.z));
        _material.SetVector("_ConeAxis", new Vector4(axis.x, axis.y, axis.z));
        _material.EnableKeyword("SPOT");

        if (Noise) _material.EnableKeyword("NOISE");
        else _material.DisableKeyword("NOISE");
        if (_light.cookie!=null) _material.SetTexture("_LightTexture0", _light.cookie);
        else _material.SetTexture("_LightTexture0", VolumetricLightRenderer.GetDefaultSpotCookie());

        var forceShadowsOff = false;
        if (Displacement.magnitude >= QualitySettings.shadowDistance) forceShadowsOff = true;

        if (_light.shadows != LightShadows.None && forceShadowsOff==false) {
            clip = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f));
            if(_reversedZ) proj = Matrix4x4.Perspective(_light.spotAngle, 1, _light.range, _light.shadowNearPlane);
            else proj = Matrix4x4.Perspective(_light.spotAngle, 1, _light.shadowNearPlane, _light.range);
            var m = clip * proj; m[0, 2] *= -1; m[1, 2] *= -1; m[2, 2] *= -1; m[3, 2] *= -1;
            _material.SetMatrix("_MyWorld2Shadow", m * view);
            _material.SetMatrix("_WorldView", m * view);
            _material.EnableKeyword("SHADOWS_DEPTH");
            _commandBuffer.SetGlobalTexture("_ShadowMapTexture", BuiltinRenderTextureType.CurrentActive);
            _commandBuffer.SetRenderTarget(renderer.GetVolumeLightBuffer());
            _commandBuffer.DrawMesh(mesh, world, _material, 0, pass);

            if (CustomRenderEvent != null) CustomRenderEvent(renderer, this, _commandBuffer, viewProj);
        } else {
            _material.DisableKeyword("SHADOWS_DEPTH");
            renderer.GlobalCommandBuffer.DrawMesh(mesh, world, _material, 0, pass);
            if (CustomRenderEvent != null) CustomRenderEvent(renderer, this, renderer.GlobalCommandBuffer, viewProj);
        }
    }


    void SetupDirectionalLight(VolumetricLightRenderer renderer, Matrix4x4 viewProj) {
        var pass = 4;
        _material.SetPass(pass);
        if (Noise) _material.EnableKeyword("NOISE");
        else _material.DisableKeyword("NOISE");
        var lpos = _light.transform.forward;
        _material.SetVector("_LightDir", new Vector4(lpos.x, lpos.y, lpos.z, 1.0f / (_light.range * _light.range)));
        _material.SetVector("_LightColor", _light.color * _light.intensity);
        _material.SetFloat("_MaxRayLength", MaxRayLength);

        if (_light.cookie != null) {
            _material.EnableKeyword("DIRECTIONAL_COOKIE");
            _material.DisableKeyword("DIRECTIONAL");
            _material.SetTexture("_LightTexture0", _light.cookie);
        } else { _material.EnableKeyword("DIRECTIONAL"); _material.DisableKeyword("DIRECTIONAL_COOKIE"); }

        // setup frustum corners for world position reconstruction
        _frustumCorners[0] = Camera.current.ViewportToWorldPoint(new Vector3(0, 0, Camera.current.farClipPlane)); // bottom left
        _frustumCorners[2] = Camera.current.ViewportToWorldPoint(new Vector3(0, 1, Camera.current.farClipPlane)); // top left
        _frustumCorners[3] = Camera.current.ViewportToWorldPoint(new Vector3(1, 1, Camera.current.farClipPlane)); // top right
        _frustumCorners[1] = Camera.current.ViewportToWorldPoint(new Vector3(1, 0, Camera.current.farClipPlane)); // bottom right
        _material.SetVectorArray("_FrustumCorners", _frustumCorners);

        Texture nullTexture = null;
        if (_light.shadows != LightShadows.None) {
            _material.EnableKeyword("SHADOWS_DEPTH");
            _commandBuffer.Blit(nullTexture, renderer.GetVolumeLightBuffer(), _material, pass);
            if (CustomRenderEvent != null) CustomRenderEvent(renderer, this, _commandBuffer, viewProj);
        } else {
            _material.DisableKeyword("SHADOWS_DEPTH");
            renderer.GlobalCommandBuffer.Blit(nullTexture, renderer.GetVolumeLightBuffer(), _material, pass);
            if (CustomRenderEvent != null) CustomRenderEvent(renderer, this, renderer.GlobalCommandBuffer, viewProj);
        }
    }


    bool IsCameraInPointLightBounds() { return Displacement.sqrMagnitude<Mathf.Pow(_light.range+1,2); }

    bool IsCameraInSpotLightBounds() {
        var distance = Vector3.Dot(_light.transform.forward, (Camera.current.transform.position - _light.transform.position));
        var extendedRange = _light.range + 1;
        if (distance > (extendedRange)) return false;
        var cosAngle = Vector3.Dot(transform.forward, (Camera.current.transform.position - _light.transform.position).normalized);
        return !((Mathf.Acos(cosAngle)*Mathf.Rad2Deg)>(_light.spotAngle+3) * 0.5f);
    }
}
