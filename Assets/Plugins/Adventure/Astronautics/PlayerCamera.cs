/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-12 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using Adventure.Astronautics;

namespace Adventure.Astronautics {
    public class PlayerCamera : Adventure.Object {
        (float x, float y) mouse = (0,0);
        float distantScale = 1000;
        string effectProfile = "DefaultAtmosphere", layer = "Distant";
        Camera mainCamera, distantCamera;
        Transform mainRoot, distantPivot, distantRoot;
        PostProcessingBehaviour effects;
        public static Vector3 Pivot {get;set;}
        public static Vector3 CameraPosition => main.transform.position;
        public static Transform Target {get;set;}
        public static Camera main => singleton?.mainCamera;
        public static PlayerCamera singleton {get;private set;}
        public static PostProcessingProfile atmosphere {
            get { return singleton.effects.profile; }
            set { singleton.effects.profile = value; } }

        public static void Reset() => Target = null;

        Camera CreateDistantCamera(float near=1, float far=15000) {
            distantRoot = new GameObject("Distant Camera").transform;
            var go = new GameObject("Camera");
            go.transform.parent = distantRoot.transform;
            var camera = go.AddComponent<Camera>();
            var effects = go.AddComponent<PostProcessingBehaviour>();
            effects.profile = Resources.Load(effectProfile) as PostProcessingProfile;
            (camera.cullingMask, camera.depth) = (1<<LayerMask.NameToLayer(layer),-2);
            (camera.useOcclusionCulling, camera.layerCullSpherical) = (false, false);
            (camera.nearClipPlane, camera.farClipPlane) = (near, far);
            (camera.allowHDR, camera.allowMSAA) = (false, false);
            // (camera.layerCullSpherical, camera.stereoMirrorMode) = (true, true);
            (camera.stereoConvergence, camera.stereoSeparation) = (0, 0);
            return camera;
        }


        void Align() => (distantRoot.position, distantRoot.rotation) =
            (mainRoot.position/distantScale, mainRoot.parent?.rotation ?? Quaternion.identity);

        void Awake() {
            if (!(singleton is null)) { Destroy(gameObject); return; }
            (singleton, effects) = (this, Get<PostProcessingBehaviour>());
            (mainCamera, distantCamera) = (Get<Camera>(), CreateDistantCamera());
            // DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(distantRoot.gameObject);
            (mainRoot, distantPivot) = (mainCamera.transform, distantCamera.transform);
            distantCamera.rect = mainCamera.rect;
            // distantCamera.fieldOfView = mainCamera.fieldOfView; // not in VR
            Align();
        }

        void LateUpdate() {
            if (!(Target is null)) { mainRoot.parent = Target; mainRoot.localPosition = Pivot; }
            Align(); // mainRoot.localRotation = Quaternion.identity;
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
                        current: transform.localPosition, target: destination,
                        currentVelocity: ref speed, maxSpeed: 299792458,
                        smoothTime: 4, deltaTime: Time.fixedDeltaTime);
                    distantPivot.position = Vector3.SmoothDamp(
                        current: distantPivot.position, target: destination/100f,
                        currentVelocity: ref distantSpeed, maxSpeed: 299792458,
                        smoothTime: 4, deltaTime: Time.fixedDeltaTime);
                }
            }
        }
    }
}
