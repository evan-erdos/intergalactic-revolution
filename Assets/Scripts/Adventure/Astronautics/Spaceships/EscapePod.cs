/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-12 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class EscapePod : SpaceObject {
        public void Jettison() {
            transform.SetParent(null);
            var rigidbody = Get<Rigidbody>();
            rigidbody.isKinematic = false;
            // rigidbody.velocity = Get<Rigidbody>().velocity;
            // rigidbody.angularVelocity = Get<Rigidbody>().angularVelocity;
            rigidbody.AddForce(transform.up*200, ForceMode.VelocityChange);
            GetComponentsInChildren<ParticleSystem>().ForEach(o => o.Play());
        }
    }
}
