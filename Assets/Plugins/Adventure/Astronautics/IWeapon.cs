/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-01 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics.Spaceships {

    /// IWeapon : IShipComponent
    /// a weapon which can be fired out into space or at something in particular
    public interface IWeapon : IShipComponent, IDamageable {

        /// Fire : () => void
        /// fires blasters at nothing in particular, default targeting
        void Fire();

        /// Fire : (target) => void
        /// fires blasters on a specified target,
        /// allowing the blaster to track position and velocity
        void Fire(ITrackable target);

        /// Fire : (position,velocity,rotation) => void
        /// fires blasters on a specified position, with a moment velocity
        void Fire(
            (float,float,float) position,
            (float,float,float) velocity);
    }
}
