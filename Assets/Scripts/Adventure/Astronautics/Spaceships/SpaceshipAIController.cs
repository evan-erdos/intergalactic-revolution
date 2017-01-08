/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Adventure.Astronautics.Spaceships {
    public class SpaceshipAIController : SpaceObject {
        bool isDisabled, canFire, isBraking;
        float perlin;
        RaycastHit[] results = new RaycastHit[10];
        List<Blaster> blasters = new List<Blaster>();
        List<BlasterTurret> turrets = new List<BlasterTurret>();
        new Rigidbody rigidbody;
        LayerMask mask;
        [SerializeField] float lateralWander = 5;
        [SerializeField] float wanderSpeed = 0.11f;
        [SerializeField] float maxClimbAngle = 45;
        [SerializeField] float maxRollAngle = 45;
        [SerializeField] float speedEffect = 0.01f;
        [SerializeField] float followRange = 10;
        [SerializeField] float aggroRange = 10000;
        [SerializeField] protected float rollEffect = 0.2f;
        [SerializeField] protected float pitchEffect = 0.5f;
        [SerializeField] protected Spaceship spaceship;
        [SerializeField] protected Transform Target;

        public void Reset() => (isDisabled,canFire) = (false,false);
        public void Disable() { isDisabled = true; spaceship.Disable(); }
        public void Fire() {
            if (PreFire()) blasters.ForEach(o => Fire(o));
            bool PreFire() =>
                canFire && Target.Get<Spaceship>().Health>0 &&
                Target.IsNear(transform,aggroRange);
        }

        protected void Fire(Blaster blaster) {
            var (rate,spread) = (1000f,0.005f);
            var (location,position) = (blaster.Barrel.ToVector(),Target.position);
            var (direction,speed) = (position-location,Target.Get<Rigidbody>().velocity);
            var distance = direction.magnitude;
            var projection = speed*distance/rate;
            var splay = Target.forward*Random.Range(-spread/2f,spread);
            var variation = (splay+Random.insideUnitSphere*spread) * distance;
            var rotation = Quaternion.LookRotation(direction);
            var (velocity,prediction) = (rigidbody.velocity,position+projection+variation);
            if (0<=Physics.RaycastNonAlloc(new Ray(location,prediction),results,100,mask))
                blaster.Fire(prediction.ToTuple(), rotation, velocity.ToTuple());
#if _DEBUG
            var directRay = new Color(1f, 1f, 1f, 0.25f);
            var predictRay = new Color(1f, 0.25f, 0.25f, 1f);
            var nearRay = new Color(1f,0.5f, 0.5f, 0.25f);
            Debug.DrawLine(location,position,directRay,1f);
            Debug.DrawLine(location,prediction,predictRay,1f);
            Debug.DrawLine(position,prediction,Color.blue,1f);
#endif
        }

        public void Move() {
            if (isDisabled || Target is null) { spaceship.Move(); return; }
            var vect = Mathf.PerlinNoise(Time.time*wanderSpeed,perlin)*2-1;
            var targetPos = Target.position + transform.right*vect*lateralWander;
            targetPos -= Target.forward*followRange;
            var localTarget = transform.InverseTransformPoint(targetPos);
            var targetAngleYaw = Mathf.Atan2(localTarget.x,localTarget.z);
            var targetAnglePitch = -Mathf.Atan2(localTarget.y,localTarget.z);
            var maxClimb = maxClimbAngle*Mathf.Deg2Rad;
            var maxRoll = maxRollAngle*Mathf.Deg2Rad;
            targetAnglePitch = Mathf.Clamp(targetAnglePitch,-maxClimb,maxClimb);
            var desiredRoll = Mathf.Clamp(targetAngleYaw,-maxRoll,maxRoll);
            var (roll,pitch,yaw) = (0f,targetAnglePitch-spaceship.PitchAngle,0f);
            var (throttle,speed) = (0.5f, 1+spaceship.ForwardSpeed*speedEffect);
            (roll,pitch,yaw) = (roll*speed*rollEffect,pitch*speed*pitchEffect,yaw*speed);
            spaceship.Move(isBraking,false,roll,pitch,yaw,throttle);
        }

        void Awake() {
            mask = LayerMask.NameToLayer("AI");
            perlin = Random.Range(0,100);
            rigidbody = spaceship.Get<Rigidbody>();
            blasters.AddRange(spaceship.GetComponentsInChildren<Blaster>());
            turrets.AddRange(spaceship.GetComponentsInChildren<BlasterTurret>());
        }

        IEnumerator Start() {
            while (!isDisabled) yield return Wait(
                wait: new WaitForSeconds(4),
                func: () => (canFire,isBraking) = (!canFire,!isBraking));
        }

        void FixedUpdate() { Move(); Fire(); }

    }
}
