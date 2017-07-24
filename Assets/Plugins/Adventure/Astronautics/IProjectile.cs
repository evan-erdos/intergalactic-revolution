/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics {

    /// IProjectile : IObject
    /// an object which can be damaged and destroyed
    public interface IProjectile : IObject, IResettable {

        /// Force : real
        /// a measure of how much damage the projectile can give
        float Force {get;}

        /// HitEvent : event
        /// raised when the projectile hits something
        event RealityAction HitEvent;

        /// Hit : () => void
        /// signifies that the projectile has been hit
        void Hit();

        /// Fire : () => void
        /// raises the fire event
        void Fire();
        void Fire(Vector3 position, Quaternion rotation, Vector3 velocity);
        void Fire(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 initial);
    }
}
