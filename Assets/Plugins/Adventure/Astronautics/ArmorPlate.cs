/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics.Spaceships {
    public class ArmorPlate : Adventure.BaseObject, IDamageable {
        [SerializeField] float health = 100;
        public float Health => health;
        public void Damage(float damage) {
            var parent = transform.parent;
            if (!gameObject || !parent) return;
            var parentRigidbody = parent.GetComponentInParent<Rigidbody>();
            if (!parentRigidbody || health<=0) return;
            if (damage>health) health -= damage;
            transform.parent = null;
            var velocity = parentRigidbody.velocity;
            var angularVelocity = parentRigidbody.angularVelocity;
            var rigidbody = GetComponent<Rigidbody>();
            if (!rigidbody) rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.isKinematic = false;
            rigidbody.velocity = velocity;
            rigidbody.angularVelocity = angularVelocity;
            rigidbody.AddForce(Random.insideUnitSphere);
        }
    }
}
