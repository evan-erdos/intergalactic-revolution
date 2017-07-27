/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using UnityEngine;

namespace Adventure.Astronautics.Spaceships {
    public class EnergyBlast : Adventure.Object, IProjectile {
        new protected Rigidbody rigidbody;
        new protected Collider collider;
        new protected Renderer renderer;
        [SerializeField] protected float damage = 50;
        [SerializeField] protected Event<CombatArgs> onHit = new Event<CombatArgs>();
        public event AdventureAction<CombatArgs> HitEvent;
        public float Damage => damage;
        public void Hit(CombatArgs e=null) => HitEvent(e ?? new CombatArgs { Sender = this, Damage = Damage });

        public virtual void Reset() {
            (renderer.enabled, collider.enabled, rigidbody.isKinematic) = (true,true,false);
            (rigidbody.velocity, rigidbody.angularVelocity) = (Vector3.zero, Vector3.zero);
            If<ParticleSystem>(o => o?.Stop()); }

        protected virtual void OnHit(CombatArgs e) {
            gameObject.SetActive(true); If<ParticleSystem>(o => o?.Play());
            (renderer.enabled, collider.enabled, rigidbody.isKinematic) = (false,false,true); }

        public void Fire() => Fire(rigidbody.position, rigidbody.velocity);
        public void Fire(Vector3 position, Vector3 velocity) => Fire(position, velocity, Vector3.zero);
        public virtual void Fire(Vector3 position, Vector3 velocity, Vector3 initial) {
            Reset(); rigidbody.position = position;
            rigidbody.rotation.SetLookRotation(position-velocity, transform.up);
            rigidbody.AddForce(initial, ForceMode.VelocityChange); rigidbody.AddForce(velocity); }

        protected virtual void Awake() {
            (rigidbody, collider, renderer) = (Get<Rigidbody>(), Get<Collider>(), Get<Renderer>());
            HitEvent += e => onHit?.Call(e); onHit.Add(e => OnHit(e));
        }

        void OnCollisionEnter(Collision c) {
            c.rigidbody?.GetParent<IDamageable>()?.Damage(Damage);
            var hit = c.contacts.First();
            transform.rotation = Quaternion.LookRotation(hit.point,hit.normal);
            Hit();
        }
    }
}
