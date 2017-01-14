/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics.Spaceships;

namespace Adventure.Astronautics {
    public class Blaster : SpaceObject, IWeapon, ICreatable<BlasterProfile> {
        [SerializeField] protected BlasterProfile profile;
        int next;
        Pool projectiles = new Pool();
        List<Transform> barrels = new List<Transform>();
        List<AudioClip> sounds = new List<AudioClip>();
        ParticleSystem flash;
        protected new AudioSource audio;
        protected new Rigidbody rigidbody;
        public bool IsDisabled {get; protected set;} = false;
        public float Health {get;protected set;} = 1000; // N
        public float Force {get;protected set;} = 4000; // N
        public float Rate {get;protected set;} = 10; // Hz
        public float Spread {get;protected set;} = 100; // m
        public float Range {get;protected set;} = 1000; // m
        public float Angle {get;protected set;} = 60; // deg
        public GameObject Projectile {get;protected set;} // object
        public Vector3 Barrel {get;protected set;}
        public ITrackable Target {get;set;}

        public virtual void Create(BlasterProfile profile) =>
            (Health, Force, Rate, Spread, Range, Angle, Projectile, sounds) =
                (profile.Health, profile.Force, profile.Rate, profile.Spread,
                profile.Range, profile.Angle, profile.Projectile, profile.sounds);

        public virtual void Disable() => IsDisabled = true;
        public virtual void Enable() => IsDisabled = false;
        public void Fire() => Fire(transform.forward);
        public void Fire(ITrackable o) => Fire(o.Position, o.Velocity);
        public void Fire(Vector3 p) => Fire(p.ToTuple(),rigidbody.velocity.ToTuple());
        public virtual void Fire(
                        (float,float,float) position,
                        (float,float,float) velocity) {
            if (!IsDisabled) StartSemaphore(Firing);
            IEnumerator Firing() {
                if (sounds.Count>0) audio.PlayOneShot(sounds.Pick(),0.8f);
                Barrel = barrels[++next%barrels.Count].position;
                if (flash) flash.Play();

                var (ratio,spray,random) = (10000,0.005f,Random.Range(-0.01f,0.01f));
                var direction = position.ToVector()-Barrel;
                var distance = direction.magnitude;
                var heading = velocity.ToVector().normalized;
                var projection = heading*distance/ratio;
                var splay = heading*random;
                var variation = (splay+Random.insideUnitSphere*spray) * distance;
                var prediction = position.ToVector()+projection+variation;
                var rotation = Quaternion.LookRotation(direction,transform.up);
                if (Quaternion.Angle(rotation,transform.rotation)>Angle/3)
                    rotation = transform.rotation;
                var rigidbody = projectiles.Create<Rigidbody>(Barrel,rotation);
                if (rigidbody.Get<IProjectile>() is GuidedMissile o) o.Target = Target;
                rigidbody.Get<IResettable>()?.Reset();
                rigidbody.transform.position = Barrel;
                rigidbody.transform.rotation = rotation;
                var forward = transform.forward*velocity.ToVector().magnitude;
                forward *= Time.fixedDeltaTime;
                rigidbody.transform.position += transform.forward*4+forward;
                rigidbody.AddForce(rigidbody.velocity+Random.insideUnitSphere*Spread);
                rigidbody.AddForce(rigidbody.transform.forward*Force);
                yield return new WaitForSeconds(1f/(Rate*barrels.Count));
            }
        }

        public void Damage(float damage) {
            Health -= damage;
            if (0<Health) return;
            (rigidbody.isKinematic, IsDisabled) = (false,true);
            transform.parent = null;
        }

        void Awake() {
            Create(profile);
            barrels.Add(transform);
            Barrel = barrels.First().position;
            (audio,rigidbody) = (GetOrAdd<AudioSource>(),GetParent<Rigidbody>());
            foreach (var particles in GetComponentsInChildren<ParticleSystem>())
                if (particles.name=="flash") flash = particles;
            for (var i=0;i<20;++i) {
                var instance = Instantiate(Projectile) as GameObject;
                instance.transform.parent = transform;
                instance.transform.localPosition = Vector3.zero;
                instance.layer = gameObject.layer;
                instance.SetActive(false);
                projectiles.Add(instance);
            }
        }
    }
}
