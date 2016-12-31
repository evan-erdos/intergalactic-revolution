/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using UnityEngine;

namespace Adventure.Astronomy.Aeronautics {

    /// ISpaceship : IDamageable
    /// a spaceship which can move around, fire weapons, and be disabled
    public interface ISpaceship : IDamageable {

        /// Disable : () => void
        /// shuts off the spaceship, alerts all children to stop
        void Disable();

        /// Move : () => void
        /// sends movement controls to the spaceship
        void Move(
            bool brakes,
            bool boost,
            float roll,
            float pitch,
            float yaw,
            float steep,
            float throttle,
            float spin);

        /// Fire : (position, velocity, rotation) => void
        /// fires blasters on a specified position,
        /// compensating for the spaceship's velocity
        void Fire();
        void Fire(
            Vector3 position,
            Vector3 velocity,
            Quaternion rotation);

        /// FireRockets : (target) => void
        /// fires missiles at a specified target
        void FireRockets();
        void FireRockets(SpaceObject target);
    }
}
