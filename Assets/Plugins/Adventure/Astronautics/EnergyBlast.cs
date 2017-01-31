/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronautics.Spaceships {
    public class EnergyBlast : SpaceObject, IProjectile {
        new protected Rigidbody rigidbody;
        new protected Collider collider;
        new protected Renderer renderer;
        [SerializeField] float force = 50;
        [SerializeField] protected SpaceEvent onHit = new SpaceEvent();
        public float Force => force;
        public event SpaceAction HitEvent;
        public void Hit() => HitEvent(this, new SpaceArgs());

        public virtual void Reset() {
            GetComponent<ParticleSystem>().Stop();
            renderer.enabled = collider.enabled = true;
            (rigidbody.isKinematic, rigidbody.velocity) = (false, Vector3.zero);
        }

        protected virtual void OnHit() {
            gameObject.SetActive(true);
            GetComponent<ParticleSystem>().Play();
            (renderer.enabled, collider.enabled) = (false,false);
            rigidbody.isKinematic = true;
        }

        protected virtual void Awake() {
            rigidbody = GetComponent<Rigidbody>();
            collider = GetComponent<Collider>();
            renderer = GetComponent<Renderer>();
            onHit.AddListener((o,e) => OnHit());
            HitEvent += onHit.Invoke;
        }

        void OnCollisionEnter(Collision c) {
            var other = c.rigidbody?.GetComponentInParent<IDamageable>();
            if (other!=null) other.Damage(Force);
            var hit = c.contacts[0];
            transform.rotation = Quaternion.LookRotation(hit.point,hit.normal);
            Hit();
        }
    }
}
