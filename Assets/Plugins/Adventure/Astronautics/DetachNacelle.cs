/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class DetachNacelle : Adventure.Object, IShipComponent, IDamageable {
        public float Health => 10000;
        public void Disable() => enabled = true;
        public void Detach() => Damage(Health+1);
        public void Damage(float damage) {
            if (!enabled || damage<Health) return;
            var rigidbody = GetOrAdd<Rigidbody>();
            var (velocity, angular) = (rigidbody.velocity, rigidbody.angularVelocity);
            (rigidbody.isKinematic, rigidbody.mass) = (false, 10);
            (rigidbody.velocity, rigidbody.angularVelocity) = (velocity, angular);
            rigidbody.AddForce(10*transform.forward, ForceMode.Impulse);
        }
    }
}
