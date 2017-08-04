/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using UnityEngine;

namespace Adventure.Astronautics.Spaceships {
    public class EnergyBlast : Adventure.Object, IProjectile {
        new protected Rigidbody rigidbody;
        new protected Collider collider;
        new protected Renderer renderer;
        float minimumSpeed = 400;
        LayerMask mask;
        [SerializeField] protected float damage = 50;
        [SerializeField] protected CombatEvent onHit = new CombatEvent();
        public event AdventureAction<CombatArgs> HitEvent;
        public float Damage => damage;
        public void Hit(CombatArgs e=null) => HitEvent(e ?? new CombatArgs { Sender=this, Damage=Damage });

        public virtual void Reset() {
            (renderer.enabled, collider.enabled, rigidbody.isKinematic) = (true,true,false);
            (rigidbody.velocity, rigidbody.angularVelocity) = (Vector3.zero, Vector3.zero);
            If<ParticleSystem>(o => o?.Stop()); }

        protected virtual void OnHit(CombatArgs e) {
            if (e.Target is IDamageable d) d.Hit(e.Damage);
            gameObject.SetActive(true); If<ParticleSystem>(o => o?.Play());
            (renderer.enabled, collider.enabled, rigidbody.isKinematic) = (false,false,true); }

        public void Fire() => Fire(rigidbody.position, rigidbody.velocity);
        public void Fire(Vector3 position, Vector3 velocity) => Fire(position,velocity,Vector3.zero);
        public virtual void Fire(Vector3 position, Vector3 velocity, Vector3 initial) {
            Reset(); rigidbody.position = position;
            rigidbody.rotation.SetLookRotation(position-velocity, transform.up);
            var minimum = (velocity.magnitude<minimumSpeed)?transform.forward*minimumSpeed:Vector3.zero;
            rigidbody.AddForce(initial+minimum, ForceMode.VelocityChange); rigidbody.AddForce(velocity);
        }

        protected virtual void Awake() {
            mask = (LayerMask.LayerToName(gameObject.layer)=="Player"
                ? LayerMask.NameToLayer("NPC") : LayerMask.NameToLayer("Player"));
            (rigidbody, collider, renderer) = (Get<Rigidbody>(), Get<Collider>(), Get<Renderer>());
            HitEvent += e => onHit?.Call(e); onHit.Add(e => OnHit(e));
        }

        void FixedUpdate() { if (Physics.Raycast(
            origin: rigidbody.position, direction: rigidbody.velocity,
            hitInfo: out var hit, layerMask: mask, maxDistance: rigidbody.velocity.magnitude))
                Hit(new CombatArgs { Sender=this, Target=hit.rigidbody?.Get<IDamageable>(),
                    Damage=Damage, Position=hit.point, Displacement=hit.normal }); }

        void OnCollisionEnter(Collision c) {
            var (rb, hit) = (c.rigidbody, c.contacts.First());
            var target = rb?.Get<IDamageable>() ?? rb?.GetParent<IDamageable>();
            transform.rotation = Quaternion.LookRotation(hit.point, hit.normal);
            Hit(new CombatArgs { Sender=this, Target=target, Damage=Damage,
                Position=hit.point, Displacement=hit.normal });
        }
    }
}
