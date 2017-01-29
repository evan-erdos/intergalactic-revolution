/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using UnityEngine;

namespace Adventure.Astronautics.Spaceships {

    /// ISpaceship : IDamageable
    /// a spaceship which can fly around and fire weapons
    public interface ISpaceship : ITrackable, IWeapon {

        /// Mass : tonnes
        /// the base mass of the ship, not including cargo
        float Mass {get;}

        /// Move : () => void
        /// sends idle movement controls to the spaceship
        void Move();

        /// Move : (real,real,real,real,real,real) => void
        /// sends movement controls to the spaceship
        void Move(
            float brakes,
            float boost,
            float throttle,
            float roll,
            float pitch,
            float yaw);

        /// HyperJump : () => void
        /// tells the ship to jump a short distance forwards
        void HyperJump();

        /// HyperJump : (quaternion) => void
        /// tells the ship to jump all the way to a new system
        void HyperJump(Quaternion direction);
    }
}
