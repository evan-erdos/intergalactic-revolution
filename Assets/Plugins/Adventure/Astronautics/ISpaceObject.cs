/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics {

    /// ISpaceObject
    /// root interface for all objects in the namespace
    public interface ISpaceObject {

        /// Name : string
        /// an identifying string for this object
        string Name {get;}

        /// Position : (real,real,real)
        /// represents the object's location in world coordinates
        Vector3 Position {get;}

        /// Create : () => void
        /// does whatever setup is required when creating an instance
        void Create();

        /// Fits : (pattern) => bool
        /// does this object match the given description?
        bool Fits(string pattern);
    }
}
