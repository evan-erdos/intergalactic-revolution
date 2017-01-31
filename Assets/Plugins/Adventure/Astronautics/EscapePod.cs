/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-12 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class EscapePod : SpaceObject {
        public void Jettison() {
            var (parent, rb) = (GetParent<Rigidbody>(), GetOrAdd<Rigidbody>());
            (transform.parent, rb.isKinematic) = (null,false);
            rb.velocity = parent.velocity;
            rb.angularVelocity = parent.angularVelocity;
            rb.AddForce(transform.up*200, ForceMode.VelocityChange);
            GetComponentsInChildren<ParticleSystem>().ForEach(o => o.Play());
        }
    }
}
