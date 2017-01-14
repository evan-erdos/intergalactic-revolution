/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics.Spaceships {

    /// IShipComponent : ISpaceObject
    /// a component of a larger ship which can be disabled
    public interface IShipComponent : ISpaceObject {

        /// Disable : () => void
        /// disables the component
        void Disable();
    }
}
