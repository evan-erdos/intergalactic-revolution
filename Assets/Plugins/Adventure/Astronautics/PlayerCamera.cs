﻿/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-12 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using Adventure.Astronautics;

namespace Adventure.Astronautics {
    public class PlayerCamera : Adventure.Object {
        (float x, float y) mouse = (0,0);
        float distantPositionScale = 1000;
        string effectProfile = "DefaultAtmosphere", layer = "Distant";
        Camera mainCamera, distantCamera;
        PostProcessingBehaviour effects;
        public static Vector3 Pivot {get;set;}
        public static Vector3 Location => main.transform.position;
        public static Transform Target {get;set;}
        public static Camera main => singleton?.mainCamera;
        public static PlayerCamera singleton {get;private set;}
        public static PostProcessingProfile atmosphere {
            get { return singleton.effects.profile; }
            set { singleton.effects.profile = value; } }

        public static void Reset() => Target = null;

        Camera CreateDistantCamera(float near=10, float far=15000) {
            var go = new GameObject("Distant Camera");
            var camera = go.AddComponent<Camera>();
            var effects = go.AddComponent<PostProcessingBehaviour>();
            effects.profile = Resources.Load(effectProfile) as PostProcessingProfile;
            (camera.cullingMask, camera.depth) = (1<<LayerMask.NameToLayer(layer),-2);
            (camera.useOcclusionCulling, camera.layerCullSpherical) = (false,false);
            (camera.nearClipPlane, camera.farClipPlane) = (near,far);
            (camera.allowHDR, camera.allowMSAA) = (false,false);
            (camera.layerCullSpherical, camera.stereoMirrorMode) = (true,true);
            (camera.stereoConvergence, camera.stereoSeparation) = (0,0);
            return camera;
        }


        void Align(Transform o) => (o.transform.position, o.transform.rotation) =
            (mainCamera.transform.position/distantPositionScale, mainCamera.transform.rotation);

        void Awake() {
            if (!(singleton is null)) { Destroy(gameObject); return; }
            (singleton, effects) = (this, Get<PostProcessingBehaviour>());
            (mainCamera, distantCamera) = (Get<Camera>(), CreateDistantCamera());
            DontDestroyOnLoad(mainCamera.gameObject);
            DontDestroyOnLoad(distantCamera.gameObject);
            distantCamera.transform.parent = transform;
            distantCamera.rect = mainCamera.rect;
            distantCamera.fieldOfView = mainCamera.fieldOfView;
            Align(distantCamera.transform);
        }

        void Update() => mouse = (Input.GetAxis("Lateral"), Input.GetAxis("Vertical"));

        // void FixedUpdate() => Align(distantCamera.transform);
        void FixedUpdate() => transform.localRotation = Quaternion.Euler(
            x: Mathf.Clamp(transform.localEulerAngles.x+mouse.y*10,-60,60),
            y: transform.localEulerAngles.y+mouse.x*10, z: 0);

        void LateUpdate() {
            if (!(Target is null)) {
                main.transform.parent = Target;
                main.transform.localPosition = Pivot;
                main.transform.localRotation = Quaternion.identity;
            } Align(distantCamera.transform);
        }

        public void Jump(Quaternion rotation) {
            StartSemaphore(Jumping);
            IEnumerator Jumping() {
                var (speed, distantSpeed) = (Vector3.zero, Vector3.zero);
                var destination = rotation*Vector3.forward*1000;
                yield return new WaitForSeconds(1);
                while (transform.localPosition!=destination) {
                    yield return new WaitForFixedUpdate();
                    transform.localPosition = Vector3.SmoothDamp(
                        current: transform.localPosition,
                        target: destination,
                        currentVelocity: ref speed,
                        smoothTime: 4,
                        maxSpeed: 299792458,
                        deltaTime: Time.fixedDeltaTime);
                    distantCamera.transform.position = Vector3.SmoothDamp(
                        current: distantCamera.transform.position, target: destination/100f,
                        currentVelocity: ref distantSpeed, maxSpeed: 299792458,
                        smoothTime: 4, deltaTime: Time.fixedDeltaTime);
                }
            }
        }
    }
}
