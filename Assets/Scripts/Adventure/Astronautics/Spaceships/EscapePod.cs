/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-12 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class EscapePod : SpaceObject {
        public void Jettison() {
            var (parent,rigidbody) = (GetParent<Rigidbody>(), GetOrAdd<Rigidbody>());
            transform.parent = null;
            rigidbody.isKinematic = false;
            rigidbody.velocity = parent.velocity;
            rigidbody.angularVelocity = parent.angularVelocity;
            rigidbody.AddForce(transform.up*200, ForceMode.VelocityChange);
            GetComponentsInChildren<ParticleSystem>().ForEach(o => o.Play());
        }
    }
}
