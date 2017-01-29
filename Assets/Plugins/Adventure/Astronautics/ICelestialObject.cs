/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics {

    /// ICelestialObject
    /// any kind of enormous stellar object
    public interface ICelestialObject {

        /// Location : (real,real,real)
        /// represents the object's location in universal coordinates
        (double x, double y, double z) Location {get;}

        /// Mass : tons
        /// represents the object's mass in solar masses
        double Mass {get;}
    }
}
