/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronomy.Aeronautics {
    public class GuidedMissile : SpaceObject {
        bool wait = true;
        new Rigidbody rigidbody;
        [SerializeField] float track = 1f;
        [SerializeField] float speed = 100f;
        [SerializeField] float time = 20f;
        [SerializeField] float damage = 100;
        [SerializeField] protected GameObject particles;

        public Transform Target {get;set;}

        IEnumerator Start() {
            rigidbody = GetComponent<Rigidbody>();
            GetComponent<Collider>().enabled = false;
            rigidbody.AddForce(transform.forward*20f, ForceMode.Acceleration);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            GetComponent<Collider>().enabled = true;
            wait = false;
            yield return new WaitForSeconds(time);
            Destroy(gameObject);
        }

        void FixedUpdate() {
            if (CastCheck()) Detonate();
            if (!Target || wait) return;
            var force = (Target.position-transform.position)*track;
            if (force.sqrMagnitude>speed) force = force.normalized * speed;
            rigidbody.AddForce(force);
            transform.rotation = Quaternion.LookRotation(rigidbody.velocity);
        }

        bool CastCheck() {
            RaycastHit hit;
            return Physics.SphereCast(
                origin: transform.position,
                radius: 2f,
                direction: transform.forward,
                hitInfo: out hit,
                maxDistance: 1f);
        }

        void Detonate() => StartCoroutine(Detonating());

        IEnumerator Detonating() {
            if (wait) yield break;
            wait = true;
            Instantiate(particles, transform.position, transform.rotation);
            yield return new WaitForSeconds(0.025f);
            Destroy(gameObject);
        }

        void OnCollisionEnter(Collision collision) {
            var damageable = collision.collider.GetComponent<IDamageable>();
            if (damageable!=null) damageable.Damage(damage);
            Detonate();
        }
    }
}
