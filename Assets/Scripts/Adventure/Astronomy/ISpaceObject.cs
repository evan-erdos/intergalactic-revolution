/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronomy {

    /// ISpaceObject
    /// root interface for all objects in the namespace
    public interface ISpaceObject {

        /// Position : (real, real, real)
        /// represents the object's location in world coordinates
        Vector3 Position {get;}
    }
}
