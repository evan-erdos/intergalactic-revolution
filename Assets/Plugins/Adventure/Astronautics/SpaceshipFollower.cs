/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class SpaceshipFollower : Adventure.Object {
        bool disabled, isFiring, isSlowing, isInFormation;
        float perlin;
        FlightArgs args = new FlightArgs();
        Vector3 formationOffset = new Vector3(-1,-1,0);
        List<Weapon> weapons = new List<Weapon>();
        Spaceship ship;

        [SerializeField] float m_PitchSensitivity = 0.5f;
        [SerializeField] float lateralWanderDistance = 5;
        [SerializeField] float lateralWanderSpeed = 0.11f;
        [SerializeField] float m_MaxClimbAngle = 45;
        [SerializeField] float m_MaxRollAngle = 45;
        [SerializeField] float m_SpeedEffect = 0.01f;
        [SerializeField] float followDistance = 10;
        [SerializeField] float dist = 500;
        [SerializeField] Transform target;
        [SerializeField] protected GameObject followTarget;

        IEnumerator Start() {
            (ship, weapons) = (Get<Spaceship>(), GetChildren<Weapon>());
            perlin = Random.Range(0f,100f);
            while (true) {
                yield return new WaitForSeconds(5);
                isInFormation = true;
                (isFiring, isSlowing) = (!isFiring, !isSlowing);
                yield return new WaitForSeconds(10);
                isInFormation = false;
            }
        }

        void FixedUpdate() {
            if (disabled) { ship.Move(); return; }
            args.Thrust = 0.5f - (isSlowing?2:0);
            var vect = Mathf.PerlinNoise(Time.time*lateralWanderSpeed,perlin)*2-1;
            var wander = isInFormation?0:lateralWanderDistance;
            if (target) args.Position = target.position-target.forward*followDistance;
            else if (followTarget) args.Position = followTarget.transform.position+formationOffset;
            else args.Position = Vector3.zero;
            args.Position += transform.right*vect*wander;
            var localTarget = transform.InverseTransformPoint(args.Position);
            var speedEffect = 1 + ship.ForwardSpeed*m_SpeedEffect;
            var (maxPitch, maxRoll) = (m_MaxClimbAngle*Mathf.Deg2Rad, m_MaxRollAngle*Mathf.Deg2Rad);
            var pitchAngle = -Mathf.Atan2(localTarget.y, localTarget.z);
            var yawAngle = Mathf.Atan2(localTarget.x, localTarget.z);
            var targetPitch = Mathf.Clamp(pitchAngle, -maxPitch, maxPitch) - ship.transform.rotation.z;
            args.Roll = Mathf.Clamp(yawAngle, -maxRoll, maxRoll)*speedEffect;
            args.Pitch = targetPitch*m_PitchSensitivity*speedEffect;
            ship.Move(args);
        }

        public void Reset() => disabled = false;
        public void Disable() { disabled = true; ship.Disable(); }
        public void SetTarget(Transform o) => this.target = o;

        public void Fire() {
            if (!PreFire()) return;
            var (rate,speed) = (1000f, target.Get<Rigidbody>().velocity);
            var (position,distance) = (target.position, target.position-transform.position);
            var velocity = Get<Rigidbody>().velocity; velocity = Vector3.zero; // haha nope
            var prediction = position+speed*distance.magnitude/rate;
            foreach (var o in weapons) o.Fire(new AttackArgs {
                Sender = this, Target = target.Get<ITrackable>(), Position = position,
                Velocity = target.Get<Rigidbody>().velocity, Displacement = velocity });
            bool PreFire() => target?.IsNear(transform,dist)==false
                || target.Get<Spaceship>().Health<0 || isFiring;
        }
    }
}
