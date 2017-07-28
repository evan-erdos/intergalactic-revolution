/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Adventure.Astronautics.Spaceships {
    public class SpaceshipAI : Adventure.Object {
        bool isDisabled, isFiring, isBraking;
        float perlin;
        new Rigidbody rigidbody;
        FlightArgs args = new FlightArgs();
        Collider[] colliders = new Collider[20];
        RaycastHit[] results = new RaycastHit[10];
        List<ITrackable> targets = new List<ITrackable>();
        List<Weapon> weapons = new List<Weapon>();
        [SerializeField] float lateralWander = 5;
        [SerializeField] float wanderSpeed = 0.11f;
        [SerializeField] float maxClimbAngle = 45;
        [SerializeField] float maxRollAngle = 45;
        [SerializeField] float speedEffect = 0.01f;
        [SerializeField] float aggroRange = 10000;
        [SerializeField] float rollEffect = 0.2f;
        [SerializeField] float pitchEffect = 0.5f;
        [SerializeField] float throttleEffect = 0.5f;
        public Spaceship ship {get;protected set;}
        public ITrackable Target {get;set;}

        public void Reset() => (isDisabled, isFiring) = (false,false);
        public void Disable() { isDisabled = true; ship.Disable(); }
        public void Fire() {
            if (PreFire()) ship.Fire(new AttackArgs { Sender=this, Target=Target });
            bool PreFire() => isFiring && Target?.Position.IsNear(transform.position, aggroRange)==true;
        }

        public void Move() {
            if (isDisabled || Target is null) { ship?.Move(); return; }
            var noise = Mathf.PerlinNoise(Time.time*wanderSpeed,perlin)*2-1;
            args.Thrust = throttleEffect - (isBraking?2:0);
            args.Position = Target.Position+transform.right*noise*lateralWander;
            var localTarget = transform.InverseTransformPoint(args.Position);
            var speed = 1+ship.ForwardSpeed*speedEffect;
            var yawAngle = Mathf.Atan2(localTarget.x,localTarget.z);
            var pitchAngle = -Mathf.Atan2(localTarget.y,localTarget.z);
            var maxClimb = maxClimbAngle*Mathf.Deg2Rad;
            var maxRoll = maxRollAngle*Mathf.Deg2Rad;
            var roll = Mathf.Clamp(yawAngle,-maxRoll,maxRoll)*speed*rollEffect;
            var pitch = Mathf.Clamp(pitchAngle,-maxClimb,maxClimb)*speed*pitchEffect;
            (args.Roll, args.Pitch, args.Yaw) = (roll, pitch, 0);
            ship?.Move(args);
        }


        IEnumerator Start() {
            perlin = Random.Range(0,100);
            if (ship is null) ship = Get<Spaceship>();
            (rigidbody,weapons) = (ship.Get<Rigidbody>(), ship.GetChildren<Weapon>());
            var (radius, mask) = (10000, 1<<LayerMask.NameToLayer("Player"));
            StartSemaphore(Toggling);

            while (true) {
                Physics.OverlapSphereNonAlloc(Position,radius,colliders,mask);
                foreach (var c in colliders) {
                    if (c==null || c?.attachedRigidbody is null) continue; yield return null;
                    if (c.attachedRigidbody?.Get<ITrackable>() is ITrackable o) targets.Add(o);
                } yield return new WaitForSeconds(2); // await 2;
                if (0<targets.Count) Target = targets.First();
            }

            IEnumerator Toggling() { while (this!=null && enabled && !isDisabled) {
                (isFiring, isBraking) = (!isFiring, !isBraking);  yield return new WaitForSeconds(8); } }
        }

        void FixedUpdate() { Move(); Fire(); }
    }
}
