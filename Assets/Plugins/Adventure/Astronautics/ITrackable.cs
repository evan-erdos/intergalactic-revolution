/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-01 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics.Spaceships {

    /// ITrackable : ISpaceObject
    /// anything which moves around and has a velocity and a position
    public interface ITrackable : ISpaceObject {

        /// Velocity : (real,real,real)
        /// current rate of speed of the ship
        Vector3 Velocity {get;}
    }
}
