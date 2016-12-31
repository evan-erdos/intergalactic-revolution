using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Effects {
    public class ExplosionPhysicsForce : MonoBehaviour {
        public float explosionForce = 4;

        IEnumerator Start() {
            yield return null;
            var multiplier = 1f;
            var r = 10*multiplier;
            var cols = Physics.OverlapSphere(transform.position, r);
            var rigidbodies = new List<Rigidbody>();
            foreach (var col in cols)
                if (col.attachedRigidbody != null
                && !rigidbodies.Contains(col.attachedRigidbody))
                    rigidbodies.Add(col.attachedRigidbody);
            foreach (var rb in rigidbodies)
                rb.AddExplosionForce(
                    explosionForce*multiplier,
                    transform.position,
                    r, 1*multiplier, ForceMode.Impulse);
        }
    }
}
