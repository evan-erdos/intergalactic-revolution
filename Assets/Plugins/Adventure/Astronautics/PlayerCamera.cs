/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-12 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using Adventure.Astronautics;

namespace Adventure.Astronautics {
    public class PlayerCamera : SpaceObject {
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

        Camera CreateDistantCamera(float near=1, float far=10000) {
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

        void Align(Transform other) =>
            (other.transform.position, other.transform.rotation) =
                (Vector3.zero, mainCamera.transform.rotation);

        void Awake() {
            if (!(singleton is null)) { Destroy(gameObject); return; }
            (singleton,effects) = (this, Get<PostProcessingBehaviour>());
            (mainCamera,distantCamera) = (Get<Camera>(),CreateDistantCamera());
            DontDestroyOnLoad(mainCamera.gameObject);
            DontDestroyOnLoad(distantCamera.gameObject);
            distantCamera.transform.parent = transform;
            distantCamera.rect = mainCamera.rect;
            distantCamera.fieldOfView = mainCamera.fieldOfView;
            Align(distantCamera.transform);
        }

        void FixedUpdate() => Align(distantCamera.transform);

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
                var speed = Vector3.zero;
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
                }
            }
        }
    }
}
