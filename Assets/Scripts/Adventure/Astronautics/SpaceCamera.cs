/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronautics {
    public class SpaceCamera : SpaceObject {
        new Camera camera;
        Camera primaryCamera;

        void Awake() => camera = Get<Camera>();
        public void Init(Camera camera) => primaryCamera = camera;

        void LateUpdate() {
            camera.rect = primaryCamera.rect;
            camera.fieldOfView = primaryCamera.fieldOfView;
            camera.transform.localPosition = Vector3.zero;
            transform.rotation = primaryCamera.transform.rotation;
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
