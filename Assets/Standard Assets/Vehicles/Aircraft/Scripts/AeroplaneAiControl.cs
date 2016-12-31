using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Vehicles.Aeroplane {
    [RequireComponent(typeof (AeroplaneController))]
    public class AeroplaneAiControl : MonoBehaviour {
        float perlin = Random.Range(0f, 100f);
        [SerializeField] float rollSensitivity = 0.2f;
        [SerializeField] float pitchSensitivity = 0.5f;
        [SerializeField] float lateralWander = 5;
        [SerializeField] float lateralWanderSpeed = 0.11f;
        [SerializeField] float maxClimbAngle = 45;
        [SerializeField] float maxRollAngle = 45;
        [SerializeField] float speedEffect = 0.01f;
        [SerializeField] Transform Target;
        AeroplaneController airplane;

        public void Reset() { }
        public void SetTarget(Transform target) { Target = target; }

        void Awake() { airplane = GetComponent<AeroplaneController>(); }
        void FixedUpdate() {
            if (Target==null) airplane.Move();
            var targetPos = Target.position + transform.right * lateralWander
                * (Mathf.PerlinNoise(Time.time*lateralWanderSpeed, perlin)*2-1);
            var localTarget = transform.InverseTransformPoint(targetPos);
            var targetAngleYaw = Mathf.Atan2(localTarget.x, localTarget.z);
            var targetAnglePitch = -Mathf.Atan2(localTarget.y, localTarget.z);

            targetAnglePitch = Mathf.Clamp(
                targetAnglePitch,
                -maxClimbAngle*Mathf.Deg2Rad,
                maxClimbAngle*Mathf.Deg2Rad);

            var changePitch = targetAnglePitch-airplane.PitchAngle;
            const float throttleInput = 0.5f;
            var pitchInput = changePitch*pitchSensitivity;
            var desiredRoll = Mathf.Clamp(
                targetAngleYaw,
                -maxRollAngle*Mathf.Deg2Rad,
                maxRollAngle*Mathf.Deg2Rad);
            var yawInput = 0f;
            var rollInput = 0f;

            yawInput = targetAngleYaw;
            rollInput = -(airplane.RollAngle - desiredRoll)*rollSensitivity;

            var currentSpeedEffect = 1f + airplane.ForwardSpeed*speedEffect;
            rollInput *= currentSpeedEffect;
            pitchInput *= currentSpeedEffect;
            yawInput *= currentSpeedEffect;

            airplane.Move(
                roll: rollInput,
                pitch: pitchInput,
                yaw: yawInput,
                throttle: throttleInput,
                airBrakes: false);
        }
    }
}
