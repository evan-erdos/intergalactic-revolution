/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics.Spaceships {
    public class ArmorPlate : Adventure.Object, IDamageable {
        float explosionForce = 100, explosionTorque = 50;
        [SerializeField] float health = 100;
        public bool IsAlive => 0<Health;
        public float Health => health;
        public void Hit(float damage=0) { if (0<(health-=damage)) Kill(); }
        public void Kill() {
            var ship = transform?.parent?.GetParent<Rigidbody>();
            if (0<health || !ship) return;
            transform.parent = null;
            var rigidbody = GetOrAdd<Rigidbody>();
            (rigidbody.useGravity, rigidbody.isKinematic) = (false,false);
            (rigidbody.velocity, rigidbody.angularVelocity) = (ship.velocity, ship.angularVelocity);
            rigidbody.AddForce(Random.insideUnitSphere*explosionForce);
            rigidbody.AddTorque(Random.insideUnitSphere*explosionTorque);
            rigidbody.AddExplosionForce(explosionForce, ship.position, 100);
        }
    }
}
