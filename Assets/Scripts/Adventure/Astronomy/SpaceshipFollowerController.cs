/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.Vehicles.Aeroplane;
using Random = UnityEngine.Random;

namespace Adventure.Astronomy.Aeronautics {
    public class SpaceshipFollowerController : MonoBehaviour {
        bool disabled, openFire, isSlowing, isInFormation;
        float perlin;
        Vector3 formationOffset = new Vector3(-1,-1,0);
        Blaster[] blasters;
        AeroplaneController controller;
        [SerializeField] float m_PitchSensitivity = 0.5f;
        [SerializeField] float lateralWanderDistance = 5;
        [SerializeField] float lateralWanderSpeed = 0.11f;
        [SerializeField] float m_MaxClimbAngle = 45;
        [SerializeField] float m_MaxRollAngle = 45;
        [SerializeField] float m_SpeedEffect = 0.01f;
        [SerializeField] float followDistance = 10f;
        [SerializeField] float dist = 500f;
        [SerializeField] Transform target;
        [SerializeField] protected GameObject followTarget;

        void Awake() {
            controller = GetComponent<AeroplaneController>();
            blasters = GetComponentsInChildren<Blaster>();
            perlin = Random.Range(0f, 100f);
        }

        IEnumerator Start() {
            while (true) {
                yield return new WaitForSeconds(5);
                isInFormation = true;
                openFire = !openFire;
                isSlowing = !isSlowing;
                yield return new WaitForSeconds(10);
                isInFormation = false;
            }
        }

        void FixedUpdate() {
            if (disabled) { controller.Move(); return; }

            // random movement vector
            var vect = Mathf.PerlinNoise(Time.time*lateralWanderSpeed,perlin)*2-1;
            // make the plane wander from the path somewhat
            var wander = isInFormation?0:lateralWanderDistance;

            // set the goal position
            var position = Vector3.zero;
            if (target) position = target.position-target.forward*followDistance;
            else if (followTarget)
                position = followTarget.transform.position+formationOffset;

            // wander a bit
            position += transform.right*vect*wander;

            // adjust the yaw and pitch towards the target
            var localTarget = transform.InverseTransformPoint(position);
            var targetAngleYaw = Mathf.Atan2(localTarget.x, localTarget.z);
            var targetAnglePitch = -Mathf.Atan2(localTarget.y, localTarget.z);

            // Set the target for the planes pitch
            targetAnglePitch = Mathf.Clamp(
                targetAnglePitch,
                -m_MaxClimbAngle*Mathf.Deg2Rad,
                m_MaxClimbAngle*Mathf.Deg2Rad);

            // calculate the difference between current pitch and desired pitch
            var changePitch = targetAnglePitch - controller.PitchAngle;

            // AI applies elevator control to reach the target angle
            // does so by modifying pitch and rotation around x
            var pitchInput = changePitch*m_PitchSensitivity;

            // clamp the planes roll
            var desiredRoll = Mathf.Clamp(
                targetAngleYaw,
                -m_MaxRollAngle*Mathf.Deg2Rad,
                m_MaxRollAngle*Mathf.Deg2Rad);
            var yawInput = 0f;
            var rollInput = 0f;

            // adjust how fast the AI is changing the controls based on the speed
            var currentSpeedEffect = 1 + controller.ForwardSpeed*m_SpeedEffect;
            rollInput *= currentSpeedEffect;
            pitchInput *= currentSpeedEffect;
            yawInput *= currentSpeedEffect;

            // pass the current input to the plane
            controller.Move(
                roll: rollInput,
                pitch: pitchInput,
                yaw: yawInput,
                throttle: 0.5f,
                airBrakes: isSlowing);

            PreFire();
        }

        public void Reset() { disabled = false; }
        public void Disable() { disabled = true; controller.Immobilize(); }

        void PreFire() {
            if (!target || !target.IsNear(transform, dist)) return;
            if (target.GetComponent<Spaceship>().Health<0) return;
            if (openFire) Fire();
        }


        public void Fire() {
            var rate = 1000f;
            var speed = target.GetComponent<Rigidbody>().velocity;
            var position = target.position;
            var distance = target.position-transform.position;
            var velocity = GetComponent<Rigidbody>().velocity;
            velocity = Vector3.zero; // haha nope
            var prediction = position+speed*distance.magnitude/rate;
            foreach (var blaster in blasters)
                blaster.Fire(
                    position: prediction,
                    velocity: velocity,
                    rotation: Quaternion.LookRotation(distance));
        }


        public void SetTarget(Transform target) { this.target = target; }
    }
}
