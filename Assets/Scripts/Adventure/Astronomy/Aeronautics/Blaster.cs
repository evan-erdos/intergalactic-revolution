/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace Adventure.Astronomy.Aeronautics {
    public class Blaster : SpaceObject, IShipComponent, IDamageable {
        bool wait, disabled;
        int next;
        RandList<AudioClip> sounds = new RandList<AudioClip>();
        List<Transform> barrels = new List<Transform>();
        ParticleSystem flash;
        [SerializeField] float delay = 0.1f;
        [SerializeField] float force = 4f;
        [SerializeField] protected GameObject projectile;
        [SerializeField] protected Transform target;
        [SerializeField] protected AudioClip[] fireSounds;
        protected new AudioSource audio;
        protected new Rigidbody rigidbody;
        public float Health {get;protected set;} = 1000;
        public Vector3 Barrel {get;protected set;}

        protected override void Awake() { base.Awake();
            barrels.Add(transform);
            Barrel = barrels.First().position;
            audio = GetComponent<AudioSource>();
            if (!audio) audio = gameObject.AddComponent<AudioSource>();
            rigidbody = GetComponentInParent<Rigidbody>();
            sounds.AddRange(fireSounds);
            foreach (var particles in GetComponentsInChildren<ParticleSystem>())
                if (particles.name=="flash") flash = particles;
        }

        public virtual void Disable() { disabled = true; }

        public void Fire() { Fire(transform.forward); }
        public void Fire(Vector3 position) { Fire(position,rigidbody.velocity); }
        public void Fire(Vector3 position, Vector3 velocity) { Fire(
            position, velocity, Quaternion.LookRotation(transform.forward)); }
        public void Fire(
                        Vector3 position,
                        Vector3 velocity,
                        Quaternion rotation) {
            Fire(projectile, position, rotation, velocity, delay, force); }

        protected virtual void Fire(
                        GameObject prefab,
                        Vector3 position,
                        Quaternion rotation,
                        Vector3 velocity,
                        float delay=0.2f,
                        float force=4f) {
            if (!wait && !disabled)
                StartCoroutine(Firing(
                    original: prefab,
                    position: position,
                    rotation: rotation,
                    velocity: velocity,
                    delay: delay,
                    force: force)); }

        public void Damage(float damage) {
            Health -= damage;
            if (0<Health) return;
            rigidbody.isKinematic = false;
            transform.parent = null;
        }


        IEnumerator Firing(
                        GameObject original,
                        Vector3 position,
                        Quaternion rotation,
                        Vector3 velocity,
                        float delay=0.2f,
                        float force=4f) {
            wait = true;
            if (sounds.Count>0) audio.PlayOneShot(sounds.Pick(),0.8f);
            Barrel = barrels[++next%barrels.Count].position;
            if (flash) flash.Play();
            var rigidbody = Extensions.Create<Rigidbody>(
                original: original,
                position: Barrel,
                rotation: rotation);
            // rigidbody.velocity = velocity;
            var forward = rigidbody.transform.forward*velocity.magnitude;
            rigidbody.transform.position += forward*Time.fixedDeltaTime;
            rigidbody.AddForce(
                force: rigidbody.velocity,
                mode: ForceMode.Force);
            rigidbody.AddForce(
                force: rigidbody.transform.forward*force,
                mode: ForceMode.Force);

            // Debug.DrawRay(transform.position,forward,Color.red,delay);
            // Debug.DrawRay(transform.position,transform.forward,Color.green,delay);

            yield return new WaitForSeconds(delay/barrels.Count);
            wait = false;
        }
    }
}
