/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronautics.Spaceships {
    public class GuidedMissile : SpaceObject, IProjectile {
        float perlin, wander = 10;
        new Rigidbody rigidbody;
        new Collider collider;
        [SerializeField] float track = 1;
        [SerializeField] float speed = 100;
        [SerializeField] float force = 100;
        [SerializeField] protected GameObject particles;
        [SerializeField] protected SpaceEvent onHit = new SpaceEvent();
        public event SpaceAction HitEvent;
        public float Force => force;
        public ITrackable Target {get;set;}
        public void Hit() => HitEvent(this,new SpaceArgs());
        public void Reset() => gameObject.SetActive(true);
        void Hit(IDamageable o) { if (o!=null) o.Damage(Force); Hit(); }
        void OnHit() { Create(particles); gameObject.SetActive(false); }

        void Awake() {
            perlin = Random.Range(1,100);
            onHit.AddListener((o,e) => OnHit());
            HitEvent += onHit.Invoke;
            rigidbody = Get<Rigidbody>();
            collider = Get<Collider>();
        }

        IEnumerator Start() {
            collider.enabled = false;
            yield return new WaitForFixedUpdate();
            collider.enabled = true;
        }

        void FixedUpdate() {
            if (Physics.Raycast(
                origin: transform.position,
                direction: transform.forward,
                hitInfo: out var hit,
                maxDistance: 1f)) Hit(hit.collider.GetParent<IDamageable>());
            if (Target is null) return;
            var displacement = Target.Position-transform.position;
            var distance = displacement.magnitude;
            var prediction = Target.Velocity.normalized*distance/speed;
            var force = (displacement+prediction)*track;
            if (force.sqrMagnitude>speed) force = force.normalized * speed;
            force += Vector3.right * Mathf.PerlinNoise(Time.time*wander,perlin);
            rigidbody.AddForce(force);
            if (rigidbody.velocity.normalized!=transform.up)
                transform.rotation = Quaternion.LookRotation(
                    rigidbody.velocity, transform.up);
        }

        void OnCollisionEnter(Collision collision) =>
            Hit(collision.collider.Get<IDamageable>());
    }
}
