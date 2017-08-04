/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics {

    /// IShield : IDamageable
    /// a protective forcefield which prevents damage from trickling up
    public interface IShield : IDamageable {

        /// Energy : real
        /// how much energy the shield has left
        float Energy {get;}
    }
}
