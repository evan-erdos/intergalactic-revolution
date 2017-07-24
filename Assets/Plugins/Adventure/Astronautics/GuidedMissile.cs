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
        [SerializeField] float force = 100;
        [SerializeField] protected GameObject particles;
        [SerializeField] protected RealityEvent onHit = new RealityEvent();
        public event RealityAction HitEvent;
        public float Force => force;
        public ITrackable Target {get;set;}
        public void Reset() => gameObject.SetActive(true);
        public void Hit() => HitEvent(this,new RealityArgs());
        void Hit(IDamageable o) { if (o!=null) o.Damage(Force); Hit(); }
        void OnHit() { Create(particles); gameObject.SetActive(false); }

        public void Fire() => Fire(rigidbody.position, rigidbody.rotation, rigidbody.velocity);
        public void Fire(Vector3 position, Quaternion rotation, Vector3 velocity) => Fire(position,rotation,velocity, Vector3.zero);
        public virtual void Fire(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 initial) {
            Reset(); (rigidbody.position, rigidbody.rotation) = (position,rotation);
            rigidbody.AddForce(initial, ForceMode.VelocityChange); rigidbody.AddForce(velocity); }


        void Awake() {
            perlin = Random.Range(1,100);
            onHit.AddListener((o,e) => OnHit());
            HitEvent += (o,e) => onHit?.Invoke(o,e);
            (rigidbody, collider) = (Get<Rigidbody>(), Get<Collider>());
        }

        IEnumerator Start() {
            collider.enabled = false;
            yield return new WaitForFixedUpdate();
            collider.enabled = true;
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
