/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics {

    /// IDamageable : ISpaceObject
    /// an object which can be damaged and destroyed
    public interface IDamageable {

        /// Health : real
        /// a measure of how much more damage the object can take
        float Health {get;}

        /// Damage : (real) => lower health
        /// removes the value of damage from this object's health
        void Damage(float damage);
    }
}
