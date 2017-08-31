/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics.Spaceships;

namespace Adventure.Astronautics {
    public class Weapon : Adventure.Object, IWeapon, ICreatable<BlasterProfile> {
        int next;
        Pool<Rigidbody> projectiles = new Pool<Rigidbody>();
        List<Transform> barrels = new List<Transform>();
        List<AudioClip> sounds = new List<AudioClip>();
        protected new AudioSource audio;
        protected new Rigidbody rigidbody;
        [SerializeField] protected BlasterProfile profile;
        [SerializeField] protected CombatEvent onFire = new CombatEvent();
        public event AdventureAction<CombatArgs> FireEvent;
        public bool IsDisabled {get;protected set;} = false;
        public float Health {get;protected set;} = 1000; // N
        public float Force {get;protected set;} = 4000; // N
        public float Rate {get;protected set;} = 10; // Hz
        public float Spread {get;protected set;} = 0.01f; // m
        public float Range {get;protected set;} = 1000; // m
        public float Angle {get;protected set;} = 30; // deg
        public GameObject Projectile {get;protected set;} // object
        public ParticleSystem particles {get;protected set;} // particles
        public Vector3 Offset => transform.forward*8; // m
        public Vector3 Barrel {get;protected set;} // position
        public ITrackable Target {get;set;} // object

        public virtual void Create(BlasterProfile o) {
            (Health, Force, Rate) = (o.Health, o.Force, o.Rate);
            (Spread, Range, Angle) = (o.Spread, o.Range, o.Angle);
            (Projectile, sounds) = (o.Projectile, o.sound.sounds);
            if (o.particles) particles = Create<ParticleSystem>(o.particles, transform);
        }

        public virtual void Disable() => IsDisabled = true;
        public virtual void Enable() => IsDisabled = false;
        public void Damage(float damage) { if ((Health-=damage)<0) Kill(); }
        public void Kill() => (rigidbody.isKinematic,transform.parent,IsDisabled) = (false,null,true);
        public virtual void Fire(CombatArgs e=null) {
            if (!IsDisabled) StartSemaphore(Firing);
            IEnumerator Firing() {
                Barrel = barrels[++next%barrels.Count].position+Offset;
                var (position, velocity, initial) = (e.Position, e.Velocity, e.Displacement);
                var (direction, random) = (position-Barrel, Random.Range(-0.01f,0.01f));
                var (distance, heading) = (direction.magnitude, e.Velocity.normalized);
                var variation = (heading*random+Random.insideUnitSphere*Spread)*distance;
                var rotation = Quaternion.LookRotation(transform.forward, transform.up);
                var projectile = projectiles.Create<IProjectile>(Barrel, rotation);
                if (projectile is GuidedMissile o) o.Target = Target;
                var time = distance/Force/projectile.Get<Rigidbody>().mass;
                var prediction = position + velocity.normalized*time + variation;
                if (prediction.magnitude>0.01)
                    rotation = Quaternion.LookRotation(prediction, transform.up);
                // if (LayerMask.LayerToName(gameObject.layer)=="Player") {
                //     Debug.DrawLine(position, prediction, Color.white, 1, true);
                //     Debug.DrawLine(Barrel, position, Color.blue, 1, true);
                //     Debug.DrawLine(Barrel, prediction, Color.red, 1, true); }
                if (!transform.IsFacing(rotation,Angle))
                    prediction = transform.forward*Force - initial/2 + variation;
                projectile?.Fire(Barrel, prediction, initial);
                FireEvent(new CombatArgs { Sender=this, Target=Target, Damage=projectile.Damage,
                    Position=Barrel, Velocity=prediction, Displacement=initial });
                yield return new WaitForSeconds(1f/(Rate*barrels.Count));
            }
        }

        void OnFire() { particles?.Play(); if (sounds.Any()) audio.PlayOneShot(sounds.Pick()); }

        void Awake() {
            Create(profile);
            barrels.Add(transform);
            (particles, Barrel) = (GetChild<ParticleSystem>(), barrels.First().position);
            (audio, rigidbody) = (Get<AudioSource>(), GetParent<Rigidbody>());
            FireEvent += e => onFire?.Call(e); onFire.Add(e => OnFire());
            projectiles = new Pool<Rigidbody>(32, () => {
                var instance = Create<Rigidbody>(Projectile);
                instance.transform.parent = transform;
                instance.transform.localPosition = Vector3.zero;
                instance.gameObject.layer = gameObject.layer;
                instance.gameObject.SetActive(false);
                return instance; });
        }
    }
}
