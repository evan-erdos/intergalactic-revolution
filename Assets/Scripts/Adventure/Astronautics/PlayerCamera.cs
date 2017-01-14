/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-12 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;

namespace Adventure.Astronautics {
    public class PlayerCamera : SpaceObject {
        [SerializeField] protected GameObject spaceCamera;
        void Start() {
            if (spaceCamera is null) return;
            Create<SpaceCamera>(spaceCamera).Init(Get<Camera>());
        }
    }
}
