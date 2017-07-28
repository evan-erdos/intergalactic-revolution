/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronautics.Spaceships {
    public class GuidedMissile : Adventure.Object, IProjectile {
        float perlin, wander = 10;
        new Rigidbody rigidbody;
        new Collider collider;
        [SerializeField] float track = 1;
        [SerializeField] float speed = 100;
        [SerializeField] float damage = 100;
        [SerializeField] protected GameObject particles;
        [SerializeField] protected CombatEvent onHit = new CombatEvent();
        public event AdventureAction<CombatArgs> HitEvent;
        public float Damage => damage;
        public ITrackable Target {get;set;}
        public void Reset() => gameObject.SetActive(true);
        public void Hit(CombatArgs e=null) => HitEvent(e ?? new CombatArgs { Sender=this, Damage=Damage });
        void Hit(IDamageable o) { if (o!=null) o.Damage(Damage); Hit(); }
        void OnHit(CombatArgs e) { Create(particles); gameObject.SetActive(false); }

        public void Fire() => Fire(rigidbody.position, rigidbody.velocity);
        public void Fire(Vector3 position, Vector3 velocity) => Fire(position, velocity, Vector3.zero);
        public virtual void Fire(Vector3 position, Vector3 velocity, Vector3 initial) {
            Reset(); rigidbody.position = position;
            rigidbody.rotation.SetLookRotation(position-velocity, transform.up);
            rigidbody.AddForce(initial, ForceMode.VelocityChange); rigidbody.AddForce(velocity); }


        void Awake() {
            (rigidbody, collider) = (Get<Rigidbody>(), Get<Collider>());
            HitEvent += e => onHit?.Call(e); onHit.Add(e => OnHit(e));
            perlin = Random.Range(1,100);
        }

        void FixedUpdate() {
            if (Physics.Raycast(transform.position, transform.forward, out var hit, 1))
                Hit(hit.collider.GetParent<IDamageable>());
            if (Target is null) return;
            var displacement = Target.Position-transform.position;
            var prediction = Target.Velocity.normalized*displacement.magnitude/speed;
            var force = (displacement+prediction)*track;
            if (force.sqrMagnitude>speed) force = force.normalized * speed;
            force += Vector3.right * Mathf.PerlinNoise(Time.time*wander,perlin);
            rigidbody.AddForce(force);
            if (rigidbody.velocity.normalized!=transform.up) transform.rotation =
                Quaternion.LookRotation(rigidbody.velocity, transform.up);
        }

        void OnCollisionEnter(Collision o) => Hit(o.collider.Get<IDamageable>());
    }
}
