/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics {

    /// ISpaceObject
    /// root interface for all objects in the namespace
    public interface ICelestialObject {

        /// Location : (real, real, real)
        /// represents the object's location in universal coordinates
        (double, double, double) Location {get;}

        /// Mass : tons
        /// represents the object's mass in solar masses
        double Mass {get;}

    }
}
