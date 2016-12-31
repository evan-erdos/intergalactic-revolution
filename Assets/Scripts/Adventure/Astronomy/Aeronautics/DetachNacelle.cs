/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronomy.Aeronautics {
    public class DetachNacelle : SpaceObject, IShipComponent, IDamageable {
        bool isOff;
        [SerializeField] bool isThruster = false;
        [SerializeField] float health = 1000f;
        public float Health => health;
        public void Disable() { isOff = true; }
        public void Detach() { if (isOff) return; isOff = true; Damage(Health+1); }
        public void Damage(float damage) {
            if (damage<Health) return;
            var velocity = GetComponentInParent<Rigidbody>().velocity;
            var angularVelocity = GetComponentInParent<Rigidbody>().angularVelocity;
            var rigidbody = GetComponent<Rigidbody>();
            if (!rigidbody) rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.isKinematic = false;
            rigidbody.velocity = velocity;
            rigidbody.angularVelocity = angularVelocity;
            rigidbody.AddForce(10*transform.forward, ForceMode.Impulse);
            if (!isThruster) return;
            var constantForce = gameObject.AddComponent<ConstantForce>();
            constantForce.relativeForce = new Vector3(-1000,0,0);
            constantForce.relativeTorque = new Vector3(0,-400,0);
        }
    }
}
