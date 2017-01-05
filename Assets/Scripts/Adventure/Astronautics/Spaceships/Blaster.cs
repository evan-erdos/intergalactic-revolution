/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics.Spaceships;

namespace Adventure.Astronautics {
    public class Blaster : SpaceObject, IWeapon {
        bool isDisabled;
        int next;
        Pool projectiles;
        RandList<AudioClip> sounds = new RandList<AudioClip>();
        List<Transform> barrels = new List<Transform>();
        ParticleSystem flash;
        [SerializeField] float delay = 0.1f;
        [SerializeField] float force = 4;
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected Transform target;
        [SerializeField] protected AudioClip[] fireSounds;
        protected new AudioSource audio;
        protected new Rigidbody rigidbody;

        public float Health {get;protected set;} = 1000;
        public (float,float,float) Barrel {get;protected set;}
        public virtual void Disable() => isDisabled = true;
        public void Fire() => Fire(transform.forward);
        public void Fire(Vector3 position) => Fire(position,rigidbody.velocity);
        public void Fire(Vector3 position, Vector3 velocity) => Fire(
            position: position.ToTuple(),
            velocity: velocity.ToTuple(),
            rotation: Quaternion.LookRotation(transform.forward));
        // public void Fire(Vector3 position, Vector3 velocity, Quaternion rotation) =>
        //     Fire(position.ToTuple(), velocity.ToTuple(), rotation, delay, force);
        public void Fire(
                        (float,float,float) position,
                        Quaternion rotation,
                        (float,float,float) velocity) =>
            Fire(position, rotation, velocity, delay, force);

        protected virtual void Fire(
                        (float x, float y, float z) position,
                        Quaternion rotation,
                        (float x, float y, float z) velocity,
                        float delay=0.2f,
                        float force=4f) {
            if (!isDisabled) StartSemaphore(Firing);
            IEnumerator Firing() {
                if (sounds.Count>0) audio.PlayOneShot(sounds.Pick(),0.8f);
                Barrel = barrels[++next%barrels.Count].position.ToTuple();
                if (flash) flash.Play();
                var rigidbody = projectiles.Create<Rigidbody>(
                    position: position.ToVector(),
                    rotation: rotation);
                rigidbody.Get<IResettable>()?.Reset();
                rigidbody.transform.position = Barrel.ToVector();
                rigidbody.transform.rotation = rotation;
                var forward = rigidbody.transform.forward*velocity.ToVector().magnitude;
                rigidbody.transform.position += forward*Time.fixedDeltaTime;
                rigidbody.AddForce(
                    force: rigidbody.velocity,
                    mode: ForceMode.Force);
                rigidbody.AddForce(
                    force: rigidbody.transform.forward*force,
                    mode: ForceMode.Force);
                yield return new WaitForSeconds(delay/barrels.Count);
            }
        }

        public void Damage(float damage) {
            Health -= damage;
            if (0<Health) return;
            (rigidbody.isKinematic, isDisabled) = (false, true);
            transform.parent = null;
        }

        void Awake() {
            barrels.Add(transform);
            Barrel = barrels.First().position.ToTuple();
            audio = GetOrAdd<AudioSource>();
            rigidbody = GetComponentInParent<Rigidbody>();
            sounds.AddRange(fireSounds);
            foreach (var particles in GetComponentsInChildren<ParticleSystem>())
                if (particles.name=="flash") flash = particles;

            var projectileInstances = new List<GameObject>();
            for (var i=0;i<10;++i) {
                var instance = Instantiate(projectilePrefab) as GameObject;
                var projectile = instance.Get<IProjectile>();
                projectileInstances.Add(instance);
                instance.transform.parent = transform;
                instance.transform.localPosition = Vector3.zero;
                instance.layer = gameObject.layer;
                instance.SetActive(false);
            } projectiles = new Pool(projectileInstances);
        }
    }
}
