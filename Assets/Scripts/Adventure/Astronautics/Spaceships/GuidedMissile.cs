/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronautics.Spaceships {
    public class GuidedMissile : SpaceObject {
        new Rigidbody rigidbody;
        new Collider collider;
        [SerializeField] float track = 1;
        [SerializeField] float speed = 100;
        [SerializeField] float force = 100;
        [SerializeField] float acceleration = 100;
        [SerializeField] protected GameObject particles;
        [SerializeField] protected SpaceEvent onHit = new SpaceEvent();
        public event SpaceAction HitEvent;
        public float Force => force;
        public Transform Target {get;set;}
        public void Hit() => HitEvent(this,new SpaceArgs());
        void OnHit() => gameObject.SetActive(false);

        void Awake() {
            onHit.AddListener((o,e) => OnHit());
            HitEvent += onHit.Invoke;
            rigidbody = Get<Rigidbody>();
            collider = Get<Collider>();
        }

        IEnumerator Start() {
            collider.enabled = false;
            rigidbody.AddForce(
                force: transform.forward*acceleration,
                mode: ForceMode.Acceleration);
            yield return new WaitForFixedUpdate();
            collider.enabled = true;
        }

        void FixedUpdate() {
            if (CastCheck()) Hit();
            if (!Target) return;
            var force = (Target.position-transform.position)*track;
            if (force.sqrMagnitude>speed) force = force.normalized * speed;
            rigidbody.AddForce(force);
            transform.rotation = Quaternion.LookRotation(rigidbody.velocity);

            bool CastCheck() => Physics.SphereCast(
                origin: transform.position,
                radius: 2f,
                direction: transform.forward,
                hitInfo: out var hit,
                maxDistance: 1f);
        }

        void OnCollisionEnter(Collision collision) {
            collision.collider.Get<IDamageable>()?.Damage(Force);
            Hit();
        }
    }
}
