/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics {

    /// IProjectile : IObject
    /// an object which can be damaged and destroyed
    public interface IProjectile : IObject, IResettable {

        /// Damage : real
        /// a measure of how much damage the projectile can give
        float Damage {get;}

        /// HitEvent : event
        /// raised when the projectile hits something
        event AdventureAction<CombatArgs> HitEvent;

        /// Hit : event
        /// signifies that the projectile has been hit
        void Hit(CombatArgs e=null);

        /// Fire : (position, velocity, initial) => void
        /// fires the projectile in a particular direction
        void Fire();
        void Fire(Vector3 position, Vector3 velocity);
        void Fire(Vector3 position, Vector3 velocity, Vector3 initial);
    }
}
