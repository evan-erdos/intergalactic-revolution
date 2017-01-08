/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using UnityEngine;

namespace Adventure.Astronautics.Spaceships {

    /// ISpaceship : IDamageable
    /// a spaceship which can fly around and fire weapons
    public interface ISpaceship : IWeapon {

        /// Velocity : (real,real,real)
        /// current rate of speed of the ship
        (float x,float y,float z) Velocity {get;}

        /// Move : () => void
        /// sends movement controls to the spaceship
        void Move();
        void Move(bool brakes,bool boost,float throttle,float roll,float pitch,float yaw);

        /// HyperJump : () => void
        /// tells the ship to jump a short distance or all the way to a new system
        void HyperJump();
        void HyperJump(Quaternion direction, StarSystem system);
    }
}
