/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-01 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics {


    /// IWeapon : IShipPart
    /// a weapon which can be fired out into space or at a specific target
    public interface IWeapon : IShipPart {


        /// Rate : real
        /// how frequently the weapon can fire
        float Rate {get;}


        /// FireEvent : event
        /// raised when the projectile is fired
        event AdventureAction<CombatArgs> FireEvent;


        /// Fire : event
        /// fires weapon at target if available, otherwise default targeting
        void Fire(CombatArgs e=null);
    }
}
