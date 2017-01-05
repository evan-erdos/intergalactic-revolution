/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics {

    /// ISpaceObject
    /// root interface for all objects in the namespace
    public interface ISpaceObject {

        /// Position : (real, real, real)
        /// represents the object's location in world coordinates
        (float x, float y, float z) Position {get;}

        /// Fits : (pattern) => bool
        /// does this object match the given description?
        bool Fits(string pattern);
    }
}
