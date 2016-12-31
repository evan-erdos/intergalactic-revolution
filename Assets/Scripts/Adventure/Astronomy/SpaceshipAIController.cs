/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Aeroplane;
using Random = UnityEngine.Random;

namespace Adventure.Astronomy.Aeronautics {
    public class SpaceshipAIController : MonoBehaviour {
        bool isDisabled, canFire, isBraking;
        [SerializeField] protected Spaceship spaceship;
        [SerializeField] protected float rollSensitivity = 0.2f;
        [SerializeField] protected float pitchSensitivity = 0.5f;
        [SerializeField] float lateralWander = 5;
        [SerializeField] float lateralWanderSpeed = 0.11f;
        [SerializeField] float maxClimbAngle = 45;
        [SerializeField] float maxRollAngle = 45;
        [SerializeField] float speedEffect = 0.01f;
        [SerializeField] float followDistance = 10;
        [SerializeField] float aggressiveDistance = 10000;
        [SerializeField] Transform Target;
        RaycastHit[] results = new RaycastHit[10];
        List<Blaster> blasters = new List<Blaster>();
        new Rigidbody rigidbody;
        float perlin;

        public void Reset() { isDisabled = false; canFire = false; }
        public void SetTarget(Transform target) { Target = target; }
        public void Disable() { isDisabled = true; spaceship.Disable(); }
        public void Fire() { if (PreFire()) blasters.ForEach(o => Fire(o)); }

        IEnumerator Start() {
            perlin = Random.Range(0,100);
            rigidbody = spaceship.GetComponent<Rigidbody>();
            blasters.AddRange(spaceship.GetComponentsInChildren<Blaster>());
            while (!isDisabled) {
                yield return new WaitForSeconds(5);
                canFire = !canFire;
                isBraking = !isBraking;
            }
        }

        void FixedUpdate() {
            if (isDisabled || Target==null) { spaceship.Move(); return; }
            var vect = Mathf.PerlinNoise(Time.time*lateralWanderSpeed, perlin)*2-1;
            var targetPos = Target.position + transform.right*vect*lateralWander;
            targetPos -= Target.forward*followDistance;
            var localTarget = transform.InverseTransformPoint(targetPos);
            var targetAngleYaw = Mathf.Atan2(localTarget.x, localTarget.z);
            var targetAnglePitch = -Mathf.Atan2(localTarget.y, localTarget.z);
            targetAnglePitch = Mathf.Clamp(
                targetAnglePitch,
                -maxClimbAngle*Mathf.Deg2Rad,
                maxClimbAngle*Mathf.Deg2Rad);
            var desiredRoll = Mathf.Clamp(
                targetAngleYaw,
                -maxRollAngle*Mathf.Deg2Rad,
                maxRollAngle*Mathf.Deg2Rad);
            var yawInput = 0f;
            var rollInput = 0f;
            var throttleInput = 0.5f;
            var pitchInput = targetAnglePitch - spaceship.PitchAngle;
            var currentSpeedEffect = 1 + (spaceship.ForwardSpeed*speedEffect);
            rollInput *= currentSpeedEffect * rollSensitivity;
            pitchInput *= currentSpeedEffect * pitchSensitivity;
            yawInput *= currentSpeedEffect;

            spaceship.Move(
                roll: rollInput,
                pitch: pitchInput,
                yaw: yawInput,
                throttle: throttleInput,
                brakes: isBraking);
            Fire();
        }


        bool PreFire() =>
            canFire && Target.GetComponent<Spaceship>().Health>0
            && Target.IsNear(transform, aggressiveDistance);


        protected void Fire(Blaster blaster) {
            var rate = 1000f;
            var spread = 0.005f;
            var mask = LayerMask.NameToLayer("AI");
            var location = blaster.Barrel;
            var position = Target.position;
            var direction = position-location;
            var velocity = Target.GetComponent<Rigidbody>().velocity;
            var distance = direction.magnitude;
            var projection = velocity*distance/rate;
            var splay = Target.forward*Random.Range(-spread/2f,spread);
            var variation = (splay+Random.insideUnitSphere*spread) * distance;
            var speed = rigidbody.velocity; speed = Vector3.zero;
            var rotation = Quaternion.LookRotation(direction);
            var prediction = position + projection + variation;
            var ray = new Ray(location,prediction);
            if (0<=Physics.RaycastNonAlloc(ray, results, 100, mask))
                blaster.Fire(prediction, speed, rotation);
#if _DEBUG
            var directRay = new Color(1f, 1f, 1f, 0.25f);
            var predictRay = new Color(1f, 0.25f, 0.25f, 1f);
            var nearRay = new Color(1f,0.5f, 0.5f, 0.25f);
            Debug.DrawLine(location,position,directRay,1f);
            Debug.DrawLine(location,prediction,predictRay,1f);
            Debug.DrawLine(position,prediction,Color.blue,1f);
#endif
        }
    }
}
