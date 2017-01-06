/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class DetachNacelle : SpaceObject, IShipComponent, IDamageable {
        [SerializeField] float health = 1000;
        public float Health => health;
        public void Disable() => enabled = true;
        public void Detach() => Damage(Health+1);
        public void Damage(float damage) {
            if (!enabled || damage<Health) return;
            var parent =  GetComponentInParent<Rigidbody>();
            var rigidbody = GetOrAdd<Rigidbody>();
            var (velocity, angularVelocity) = (parent.velocity,parent.angularVelocity);
            rigidbody.mass = 10;
            (rigidbody.useGravity, rigidbody.isKinematic) = (false, false);
            (rigidbody.velocity, rigidbody.angularVelocity) = (velocity, angularVelocity);
            rigidbody.AddForce(10*transform.forward, ForceMode.Impulse);
        }
    }
}
