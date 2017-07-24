/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics.Spaceships {
    public class ArmorPlate : Adventure.Object, IDamageable {
        float explosionForce = 100, explosionTorque = 50;
        [SerializeField] float health = 100;
        public float Health => health;
        public void Damage(float damage) {
            var parent = transform.parent;
            if (!gameObject || !parent) return;
            var ship = parent.GetParent<Rigidbody>();
            if (!ship || health<=0) return;
            if (damage>health) health -= damage;
            transform.parent = null;
            var rigidbody = GetOrAdd<Rigidbody>();
            (rigidbody.useGravity, rigidbody.isKinematic) = (false,false);
            (rigidbody.velocity, rigidbody.angularVelocity) = (ship.velocity, ship.angularVelocity);
            rigidbody.AddForce(Random.insideUnitSphere*explosionForce);
            rigidbody.AddTorque(Random.insideUnitSphere*explosionTorque);
        }
    }
}
