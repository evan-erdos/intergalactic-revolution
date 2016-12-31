/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronomy.Aeronautics {
    public class EnergyBlast : SpaceObject {
        RaycastHit hit;
        new protected Rigidbody rigidbody;
        [SerializeField] float damage = 50;
        [SerializeField] protected GameObject particles;

        IEnumerator Start() {
            rigidbody = GetComponent<Rigidbody>();
            GetComponent<Collider>().enabled = false;
            yield return new WaitForFixedUpdate();
            GetComponent<Collider>().enabled = true;
            yield return new WaitForSeconds(10f);
            Destroy(gameObject);
        }

        public void Detonate() => StartSemaphore(Detonating);
        IEnumerator Detonating() {
            Instantiate(particles, transform.position, transform.rotation);
            yield return new WaitForSeconds(0.025f);
            Destroy(gameObject);
        }

    	void OnCollisionEnter(Collision c) {
            var other = c.rigidbody?.GetComponentInParent<IDamageable>();
            if (other!=null) other.Damage(damage);
            Detonate();
        }
    }
}
