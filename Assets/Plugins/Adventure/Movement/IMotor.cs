/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-06-21 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Movement {

    /// IMotor : interface
    /// any movement system which can be given movement commands
    public interface IMotor : IObject {

        /// IsGrounded : bool
        /// is the motor currently grounded?
        bool IsGrounded {get;}

        /// IsSliding : bool
        /// is the motor sliding?
        bool IsSliding {get;}

        /// IsSprinting : bool
        /// is the motor sprinting?
        bool IsSprinting {get;}

        /// IsJumping : bool
        /// is the motor jumping?
        bool IsJumping {get;}

        /// WasJumping : bool
        /// was the motor jumping last frame?
        bool WasJumping {get;}

        /// IsDisabled : bool
        /// is the motor responsive?
        bool IsDisabled {get;set;}

        /// Velocity : (real,real,real)
        /// current velocity of this motor
        Vector3 Velocity {get;}

        /// Move : ((real,real,real), bool, bool) => void
        /// moves the motor
        void Move(Vector3 move, bool duck, bool jump);

    }
}
