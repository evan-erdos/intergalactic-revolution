/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics.Spaceships;

namespace Adventure.Astronautics {
    public class Weapon : Adventure.Object, IWeapon, ICreatable<BlasterProfile> {
        [SerializeField] protected BlasterProfile profile;
        int next;
        Pool<Rigidbody> projectiles = new Pool<Rigidbody>();
        List<Transform> barrels = new List<Transform>();
        List<AudioClip> sounds = new List<AudioClip>();
        protected new AudioSource audio;
        protected new Rigidbody rigidbody;
        public bool IsDisabled {get;protected set;} = false;
        public float Health {get;protected set;} = 1000; // N
        public float Force {get;protected set;} = 4000; // N
        public float Rate {get;protected set;} = 10; // Hz
        public float Spread {get;protected set;} = 100; // m
        public float Range {get;protected set;} = 1000; // m
        public float Angle {get;protected set;} = 60; // deg
        public GameObject Projectile {get;protected set;} // object
        public ParticleSystem particles {get;protected set;} // particles
        public Vector3 Barrel {get;protected set;} // position
        public ITrackable Target {get;set;} // object

        public virtual void Create(BlasterProfile profile) =>
            (Health, Force, Rate, Spread, Range, Angle, Projectile, sounds) =
                (profile.Health, profile.Force, profile.Rate,
                profile.Spread, profile.Range, profile.Angle,
                profile.Projectile, profile.sounds);

        public virtual void Disable() => IsDisabled = true;
        public virtual void Enable() => IsDisabled = false;
        public void Damage(float damage) => If ((Health-=damage)<0, () => Kill());
        public void Kill() => (rigidbody.isKinematic, transform.parent, IsDisabled) = (false,null,true);
        public void Fire() => Fire(transform.forward);
        public void Fire(Vector3 position) => Fire(position.tuple(), (0,0,0), (0,0,0));
        public void Fire(ITrackable o) => Fire(o.Position.tuple(), o.Velocity.tuple(), (0,0,0));
        public void Fire((float,float,float) position, (float,float,float) velocity) => Fire(position,velocity,(0,0,0));
        public virtual void Fire((float,float,float) position, (float,float,float) velocity, (float,float,float) initial) {
            if (!IsDisabled) StartSemaphore(Firing);
            IEnumerator Firing() {
                if (sounds.Any()) audio.PlayOneShot(sounds.Pick(),0.8f);
                Barrel = barrels[++next%barrels.Count].position;
                particles?.Play();

                var (ratio, spray) = (10000, 0.005f);
                var (direction, random) = (position.vect()-Barrel, Random.Range(-0.01f,0.01f));
                var (distance, heading) = (direction.magnitude, velocity.vect().normalized);
                var variation = (heading*random+Random.insideUnitSphere*spray)*distance;
                var prediction = position.vect()+heading*distance/ratio+variation;
                var rotation = Quaternion.LookRotation(direction,transform.up);
                if (Quaternion.Angle(rotation,transform.rotation)>Angle/3) rotation = transform.rotation;
                var rigidbody = projectiles.Create(Barrel,rotation);
                if (rigidbody.Get<IProjectile>() is GuidedMissile o) o.Target = Target;
                rigidbody.Get<IResettable>()?.Reset();
                (rigidbody.transform.position, rigidbody.transform.rotation) = (Barrel, rotation);
                var forward = transform.forward*velocity.vect().magnitude * Time.fixedDeltaTime;
                rigidbody.transform.position += transform.forward*4+forward;
                rigidbody.AddForce(initial, ForceMode.VelocityChange);
                rigidbody.AddForce(rigidbody.velocity+Random.insideUnitSphere*Spread);
                rigidbody.AddForce(rigidbody.transform.forward*Force);
                yield return new WaitForSeconds(1f/(Rate*barrels.Count));
            }
        }

        void Awake() {
            Create(profile);
            barrels.Add(transform);
            Barrel = barrels.First().position;
            (audio, rigidbody) = (Get<AudioSource>(), GetParent<Rigidbody>());
            particles = GetChild<ParticleSystem>();
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
