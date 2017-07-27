/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics.Spaceships;

namespace Adventure.Astronautics {
    public class Weapon : Adventure.Object, IWeapon, ICreatable<BlasterProfile> {
        [SerializeField] protected BlasterProfile profile;
        [SerializeField] protected Event<AttackArgs> onFire = new Event<AttackArgs>();
        int next;
        Pool<Rigidbody> projectiles = new Pool<Rigidbody>();
        List<Transform> barrels = new List<Transform>();
        List<AudioClip> sounds = new List<AudioClip>();
        protected new AudioSource audio;
        protected new Rigidbody rigidbody;
        public event AdventureAction<AttackArgs> FireEvent;
        public bool IsDisabled {get;protected set;} = false;
        public float Health {get;protected set;} = 1000; // N
        public float Force {get;protected set;} = 4000; // N
        public float Rate {get;protected set;} = 10; // Hz
        public float Spread {get;protected set;} = 0.01f; // m
        public float Range {get;protected set;} = 1000; // m
        public float Angle {get;protected set;} = 30; // deg
        public GameObject Projectile {get;protected set;} // object
        public ParticleSystem particles {get;protected set;} // particles
        public Vector3 Barrel {get;protected set;} // position
        public ITrackable Target {get;set;} // object

        public virtual void Create(BlasterProfile o) =>
            (Health, Force, Rate, Spread, Range, Angle, Projectile, sounds) =
                (o.Health, o.Force, o.Rate, o.Spread, o.Range, o.Angle, o.Projectile, o.sound.sounds);

        public virtual void Disable() => IsDisabled = true;
        public virtual void Enable() => IsDisabled = false;
        public void Damage(float damage) => If ((Health-=damage)<0, () => Kill());
        public void Kill() => (rigidbody.isKinematic,transform.parent,IsDisabled) = (false,null,true);
        public virtual void Fire(AttackArgs e=null) {
            if (!IsDisabled) StartSemaphore(Firing);
            IEnumerator Firing() {
                Barrel = barrels[++next%barrels.Count].position;
                var (position, velocity, initial) = (e.Position, e.Velocity, e.Displacement);
                var (direction, random) = (position-Barrel, Random.Range(-0.01f,0.01f));
                var (distance, heading) = (direction.magnitude, e.Velocity.normalized);
                var variation = (heading*random+Random.insideUnitSphere*Spread)*distance;
                var rotation = Quaternion.LookRotation(transform.forward, transform.up);
                var projectile = projectiles.Create<IProjectile>(Barrel, rotation);
                if (projectile is GuidedMissile o) o.Target = Target;
                var start = Barrel+transform.forward*Time.fixedDeltaTime;
                var time = distance/Force/projectile.Get<Rigidbody>().mass;
                var prediction = position+velocity.normalized*time;
                var solution = start - position + prediction + variation;
                rotation = Quaternion.LookRotation(solution, transform.up);
                // if (name=="Diamond Spray Cannon") {
                //     Debug.DrawLine(position, solution, Color.white, 1, true);
                //     Debug.DrawLine(start, position, Color.blue, 1, true);
                //     Debug.DrawLine(start, solution, Color.red, 1, true); }
                if (!transform.IsFacing(rotation,Angle))
                    solution = transform.forward*Force + rigidbody.velocity + variation;
                projectile?.Fire(start, solution, initial);
                FireEvent(new AttackArgs {
                    Sender = this, Target = Target, Damage = projectile.Damage,
                    Position = start, Velocity = solution, Displacement = initial });
                yield return new WaitForSeconds(1f/(Rate*barrels.Count));
            }
        }

        void OnFire() { particles?.Play(); if (sounds.Any()) audio.PlayOneShot(sounds.Pick(),0.8f); }

        void Awake() {
            Create(profile);
            barrels.Add(transform);
            (particles, Barrel) = (GetChild<ParticleSystem>(), barrels.First().position);
            (audio, rigidbody) = (Get<AudioSource>(), GetParent<Rigidbody>());
            onFire.Add(e => OnFire()); FireEvent += e => onFire?.Call(e);
            projectiles = new Pool<Rigidbody>(10, () => {
                var instance = Create<Rigidbody>(Projectile);
                instance.transform.parent = transform;
                instance.transform.localPosition = Vector3.zero;
                instance.gameObject.layer = gameObject.layer;
                instance.gameObject.SetActive(false);
                return instance; });
        }
    }
}
