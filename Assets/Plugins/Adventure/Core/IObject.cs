/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-10 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure {

    /// IObject : IDescribable
    /// provides a common interface to every object
    public interface IObject {

        /// Name : string
        /// an identifying string for this object
        string Name {get;}

        /// Position : (real,real,real)
        /// represents the object's location in world coordinates
        Vector3 Position {get;}

        /// Create : () => void
        /// does whatever local setup is required when creating an instance
        void Create();

        /// Fits : (string) => bool
        /// determines if the string matches the pattern for this object
        bool Fits(string pattern);

        /// Get : <T>() => T
        /// gets the component T if such a component is attached, else null
        T Get<T>();

        /// Find : <T>() => Thing[]
        /// a collections of all nearby things within the default range
        List<T> Find<T>(float r=5) where T : IThing;
    }
}
