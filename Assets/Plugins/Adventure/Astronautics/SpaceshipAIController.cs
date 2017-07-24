/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Adventure.Astronautics.Spaceships {
    public class SpaceshipAIController : Adventure.Object {
        bool isDisabled, isFiring, isBraking;
        float perlin;
        new Rigidbody rigidbody;
        Collider[] colliders = new Collider[20];
        RaycastHit[] results = new RaycastHit[10];
        List<ITrackable> targets = new List<ITrackable>();
        List<Weapon> weapons = new List<Weapon>();
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
        public Spaceship spaceship {get;protected set;}
        public ITrackable Target {get;set;}

        public void Reset() => (isDisabled, isFiring) = (false,false);
        public void Disable() { isDisabled = true; spaceship.Disable(); }
        public void Fire() {
            if (PreFire()) spaceship.Fire(Target);
            bool PreFire() => isFiring && !(Target is null) && Target.Position.IsNear(transform.position,aggroRange);
        }

        public void Move() {
            if (isDisabled || Target is null) { spaceship?.Move(); return; }
            var (brakes, boost, yawEffect) = (0f,0f,1f);
            var vect = Mathf.PerlinNoise(Time.time*wanderSpeed,perlin)*2-1;
            var goal = Target.Position+transform.right*vect*lateralWander;
            // goal -= Target.forward*followRange;
            // if (Physics.SphereCast(transform.position, radius, transform.forward, out var hit, 100)) goal = whatever
            var localTarget = transform.InverseTransformPoint(goal);
            var yawAngle = Mathf.Atan2(localTarget.x,localTarget.z);
            var pitchAngle = -Mathf.Atan2(localTarget.y,localTarget.z);
            var maxClimb = maxClimbAngle*Mathf.Deg2Rad;
            var maxRoll = maxRollAngle*Mathf.Deg2Rad;
            pitchAngle = Mathf.Clamp(pitchAngle,-maxClimb,maxClimb);
            var desiredRoll = Mathf.Clamp(yawAngle,-maxRoll,maxRoll);
            var speed = 1+spaceship.ForwardSpeed*speedEffect;
            var (roll, pitch, yaw) = (speed,pitchAngle*speed,speed);
            roll *= rollEffect; pitch *=pitchEffect; yaw *= yawEffect;
            spaceship?.Move(brakes,boost,throttleEffect,roll,pitch,yaw);
        }

        void Awake() {
            (perlin, mask) = (Random.Range(0,100), 1<<LayerMask.NameToLayer("AI"));
            if (!spaceship) spaceship = Get<Spaceship>();
            rigidbody = spaceship.Get<Rigidbody>();
            weapons.Add(spaceship.GetComponentsInChildren<Weapon>());
        }

        async void Start() {
            var (radius, layerMask) = (10000, 1<<LayerMask.NameToLayer("Player"));
            StartAsync(Toggling);

            while (true) {
                await Physics.OverlapSphereNonAlloc(Position,radius,colliders,layerMask);
                foreach (var c in colliders) {
                    if (c?.attachedRigidbody is null) continue; await 0;
                    if (c.attachedRigidbody?.Get<ITrackable>() is ITrackable o) targets.Add(o);
                } await 2; // if (0<targets.Count) Target = targets.First();
            }

            async Task Toggling() { while (!isDisabled) { await 4; (isFiring,isBraking) = (!isFiring,!isBraking); } }
        }

        void FixedUpdate() { Move(); Fire(); }
    }
}
