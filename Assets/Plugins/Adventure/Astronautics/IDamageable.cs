/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics {

    /// IDamageable : IObject
    /// an object which can be damaged and destroyed
    public interface IDamageable : IObject {

        /// IsAlive : bool
        /// if the thing is still functional / alive or not
        bool IsAlive {get;}

        /// Health : real
        /// a measure of how much more damage the object can take
        float Health {get;}

        /// Damage : (real) => lower health
        /// removes the value of damage from this object's health
        void Hit(float damage=0);
    }
}
