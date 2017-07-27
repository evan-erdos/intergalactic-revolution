/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-01 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics.Spaceships {

    /// IWeapon : IShipComponent
    /// a weapon which can be fired out into space or at a specific target
    public interface IWeapon : IShipComponent {

        /// FireEvent : event
        /// raised when the projectile is fired
        event AdventureAction<AttackArgs> FireEvent;

        /// Fire : event
        /// fires weapon at target if available, otherwise default targeting
        void Fire(AttackArgs e=null);
    }
}
