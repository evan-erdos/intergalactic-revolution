/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-13 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure {

    /// ICreatable
    /// anything which can be initialized with a scriptable object
    public interface ICreatable<T> where T : ScriptableObject {

        /// Create : (profile) => serialized game object
        /// applies the data in the data object to the object
        void Create(T profile);

        /// Create : () => initialized game object
        /// should be called anytime the object is created
        void Create();
    }
}
