/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using UnityEngine;

namespace Adventure.Astronautics.Spaceships {
    public class EnergyBlast : Adventure.Object, IProjectile {
        new protected Rigidbody rigidbody;
        new protected Collider collider;
        new protected Renderer renderer;
        [SerializeField] protected float force = 50;
        [SerializeField] protected RealityEvent onHit = new RealityEvent();
        public float Force => force;
        public event RealityAction HitEvent;
        public void Hit() => HitEvent(this, new RealityArgs());

        public virtual void Reset() {
            If<ParticleSystem>(o => o?.Stop());
            (renderer.enabled, collider.enabled) = (true,true);
            (rigidbody.isKinematic, rigidbody.velocity) = (false, Vector3.zero);
        }

        protected virtual void OnHit() {
            gameObject.SetActive(true);
            If<ParticleSystem>(o => o?.Play());
            (renderer.enabled, collider.enabled) = (false,false);
            rigidbody.isKinematic = true;
        }

        protected virtual void Awake() {
            (rigidbody,collider,renderer) = (Get<Rigidbody>(), Get<Collider>(), Get<Renderer>());
            onHit.AddListener((o,e) => OnHit());
            HitEvent += onHit.Invoke;
        }

        void OnCollisionEnter(Collision c) {
            c.rigidbody?.GetParent<IDamageable>()?.Damage(Force);
            var hit = c.contacts.First();
            transform.rotation = Quaternion.LookRotation(hit.point,hit.normal);
            Hit();
        }
    }
}
