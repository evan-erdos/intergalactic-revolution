/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-01 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics.Spaceships {

    /// IWeapon : IShipComponent
    /// a weapon on a space ship
    public interface IWeapon : IShipComponent, IDamageable {

        /// Fire : (position, velocity, rotation) => void
        /// fires blasters on a specified position,
        /// compensating for the spaceship's velocity
        void Fire();
        void Fire(
            (float, float, float) position,
            Quaternion rotation,
            (float, float, float) velocity);
    }
}
