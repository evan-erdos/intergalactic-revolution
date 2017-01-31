/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Adventure.Astronautics.Spaceships {
    public class SpaceshipAIController : SpaceObject {
        bool isDisabled, isFiring, isBraking;
        float perlin;
        Collider[] colliders = new Collider[20];
        RaycastHit[] results = new RaycastHit[10];
        List<ITrackable> targets = new List<ITrackable>();
        List<Weapon> weapons = new List<Weapon>();
        new Rigidbody rigidbody;
        LayerMask mask;
        [SerializeField] float lateralWander = 5;
        [SerializeField] float wanderSpeed = 0.11f;
        [SerializeField] float maxClimbAngle = 45;
        [SerializeField] float maxRollAngle = 45;
        [SerializeField] float speedEffect = 0.01f;
        [SerializeField] float aggroRange = 10000;
        [SerializeField] float rollEffect = 0.2f;
        [SerializeField] float pitchEffect = 0.5f;
        [SerializeField] float throttleEffect = 0.5f;
        [SerializeField] protected Spaceship spaceship;
        public ITrackable Target {get;protected set;}

        public void Reset() => (isDisabled, isFiring) = (false,false);
        public void Disable() { isDisabled = true; spaceship.Disable(); }
        public void Fire() {
            if (PreFire()) spaceship.Fire(Target);
            bool PreFire() =>
                isFiring && !(Target is null) &&
                Target.Position.IsNear(transform.position,aggroRange);
        }

        public void Move() {
            if (isDisabled || Target is null) { spaceship.Move(); return; }
            var (brakes, boost, yawEffect) = (0f,0f,1f);
            var vect = Mathf.PerlinNoise(Time.time*wanderSpeed,perlin)*2-1;
            var goal = Target.Position+transform.right*vect*lateralWander;
            // goal -= Target.forward*followRange;
            // if (Physics.SphereCast(
            //     origin: transform.position,
            //     radius: radius,
            //     direction: transform.forward,
            //     hitInfo: out var hit,
            //     maxDistance: 100)) goal = whatever
            var localTarget = transform.InverseTransformPoint(goal);
            var yawAngle = Mathf.Atan2(localTarget.x,localTarget.z);
            var pitchAngle = -Mathf.Atan2(localTarget.y,localTarget.z);
            var maxClimb = maxClimbAngle*Mathf.Deg2Rad;
            var maxRoll = maxRollAngle*Mathf.Deg2Rad;
            pitchAngle = Mathf.Clamp(pitchAngle,-maxClimb,maxClimb);
            var desiredRoll = Mathf.Clamp(yawAngle,-maxRoll,maxRoll);
            var (roll,pitch,yaw) = (0f,pitchAngle,0f);
            var speed = 1+spaceship.ForwardSpeed*speedEffect;
            roll = roll*rollEffect*speed;
            pitch = pitch*pitchEffect*speed;
            yaw = yaw*yawEffect*speed;
            spaceship.Move(brakes,boost,throttleEffect,roll,pitch,yaw);
        }

        void Awake() {
            mask = 1<<LayerMask.NameToLayer("AI");
            perlin = Random.Range(0,100);
            if (!spaceship) spaceship = Get<Spaceship>();
            rigidbody = spaceship.Get<Rigidbody>();
            weapons.Add(spaceship.GetComponentsInChildren<Weapon>());
        }

        IEnumerator Start() {
            var radius = 10000;
            var layerMask = 1<<LayerMask.NameToLayer("Player");
            StartCoroutine(Toggling());
            while (true) {
                yield return new WaitForSeconds(2);
                Physics.OverlapSphereNonAlloc(Position,radius,colliders,layerMask);
                foreach (var result in colliders) {
                    yield return null;
                    if (result?.attachedRigidbody is null) continue;
                    var ship = result.attachedRigidbody.Get<ITrackable>();
                    if (!(ship is null)) targets.Add(ship);
                } yield return null;
                // if (0<targets.Count) Target = targets.First();
            }

            IEnumerator Toggling() {
                while (!isDisabled) yield return Wait(
                    wait: new WaitForSeconds(4),
                    func: () => (isFiring,isBraking) = (!isFiring,!isBraking));
            }
        }

        void FixedUpdate() { Move(); Fire(); }
    }
}
