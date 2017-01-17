/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Collections;
using UnityEngine;
// using UnityStandardAssets.Vehicles.Aeroplane;
using Random = UnityEngine.Random;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class SpaceshipFollower : MonoBehaviour {
        bool disabled, openFire, isSlowing, isInFormation;
        float perlin;
        Vector3 formationOffset = new Vector3(-1,-1,0);
        Weapon[] weapons;
        Spaceship controller;
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
            controller = GetComponent<Spaceship>();
            weapons = GetComponentsInChildren<Weapon>();
            perlin = Random.Range(0f,100f);
        }

        IEnumerator Start() {
            while (true) {
                yield return new WaitForSeconds(5);
                isInFormation = true;
                (openFire, isSlowing) = (!openFire, !isSlowing);
                yield return new WaitForSeconds(10);
                isInFormation = false;
            }
        }

        void FixedUpdate() {
            if (disabled) { controller.Move(); return; }
            var (brakes, boost, throttle) = (isSlowing?1f:0, 0, 0.5f);
            var vect = Mathf.PerlinNoise(Time.time*lateralWanderSpeed,perlin)*2-1;
            var wander = isInFormation?0:lateralWanderDistance;
            var position = Vector3.zero;
            if (target) position = target.position-target.forward*followDistance;
            else if (followTarget)
                position = followTarget.transform.position+formationOffset;
            position += transform.right*vect*wander;
            var localTarget = transform.InverseTransformPoint(position);
            var targetAngleYaw = Mathf.Atan2(localTarget.x, localTarget.z);
            var targetAnglePitch = -Mathf.Atan2(localTarget.y, localTarget.z);
            targetAnglePitch = Mathf.Clamp(
                targetAnglePitch,
                -m_MaxClimbAngle*Mathf.Deg2Rad,
                m_MaxClimbAngle*Mathf.Deg2Rad);

            var changePitch = targetAnglePitch - controller.transform.rotation.z;
            var pitch = changePitch*m_PitchSensitivity;
            var desiredRoll = Mathf.Clamp(
                targetAngleYaw,
                -m_MaxRollAngle*Mathf.Deg2Rad,
                m_MaxRollAngle*Mathf.Deg2Rad);
            var (yaw, roll) = (0f,0f);
            var currentSpeedEffect = 1 + controller.ForwardSpeed*m_SpeedEffect;
            roll *= currentSpeedEffect;
            pitch *= currentSpeedEffect;
            yaw *= currentSpeedEffect;
            controller.Move(brakes,boost,throttle,roll,pitch,yaw);
        }

        public void Reset() => disabled = false;
        public void Disable() { disabled = true; controller.Disable(); }
        public void SetTarget(Transform target) => this.target = target;

        public void Fire() {
            if (!PreFire()) return;
            var rate = 1000f;
            var speed = target.GetComponent<Rigidbody>().velocity;
            var position = target.position;
            var distance = target.position-transform.position;
            var velocity = GetComponent<Rigidbody>().velocity;
            velocity = Vector3.zero; // haha nope
            var prediction = position+speed*distance.magnitude/rate;
            foreach (var weapon in weapons)
                weapon.Fire(prediction.tuple(), velocity.tuple());
            bool PreFire() =>
                !target || !target.IsNear(transform, dist) ||
                target.Get<Spaceship>().Health<0 || openFire;
        }
    }
}
