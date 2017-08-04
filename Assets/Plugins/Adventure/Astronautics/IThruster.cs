/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics {


    /// IThruster : IShipPart
    /// a component of a larger ship which can be disabled
    public interface IThruster : IShipPart {


        /// SetShip : (ship) => void
        /// assigns spaceship if parent isn't available
        void SetShip(Spaceships.ISpaceship ship);
    }
}
