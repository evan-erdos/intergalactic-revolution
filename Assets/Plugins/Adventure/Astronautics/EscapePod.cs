/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-12 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class EscapePod : Adventure.Object {
        public void Jettison() {
            var (parent, rb) = (GetParent<Rigidbody>(), GetOrAdd<Rigidbody>());
            (transform.parent, rb.isKinematic) = (null,false);
            (rb.velocity, rb.angularVelocity) = (parent.velocity, parent.angularVelocity);
            rb.AddForce(transform.up*200, ForceMode.VelocityChange);
            GetChildren<ParticleSystem>().ForEach(o => o.Play());
        }
    }
}
