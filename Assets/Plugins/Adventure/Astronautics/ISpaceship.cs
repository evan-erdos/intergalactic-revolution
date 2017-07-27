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

        /// MoveEvent : event
        /// event for flight motion
        event AdventureAction<FlightArgs> MoveEvent;

        /// Move : event
        /// raises movement event and sends controls to the ship
        void Move(FlightArgs e=null);

        /// HyperJump : () => void
        /// tells the ship to jump a short distance forwards
        void HyperJump();

        /// HyperJump : (quaternion) => void
        /// tells the ship to jump all the way to a new system
        void HyperJump(Quaternion direction);
    }
}
