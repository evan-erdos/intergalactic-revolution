/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-11-02 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure {

    /// IActor : IThing
    /// anything which can take damage and be killed
    public interface IActor : IThing {

        /// IsDead : bool
        /// is this being dead?
        bool IsDead {get;}

        /// Health : real
        /// how much health does the actor have left
        decimal Health {get;}

        /// Vitality : real
        /// how much vitality does the actor have, unscathed
        decimal Vitality {get;}

        /// KillEvent : event
        /// raised when the actor is killed
        event StoryAction KillEvent;

        /// GotoEvent : event
        /// raised when the actor goes to a thing or into a room
        event StoryAction GotoEvent;

        /// Hurt : (damage) => void
        /// applies damage to the actor, potentially killing it
        /// - damage : real
        ///     damage to be dealt (or health to be added)
        void Hurt(decimal damage);
    }
}
