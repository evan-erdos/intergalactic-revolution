/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics {

    /// IResettable : ISpaceObject
    /// an object which can be damaged and destroyed
    public interface IResettable {

        /// Reset : () => fixed
        /// puts everything back to normal
        void Reset();
    }
}
