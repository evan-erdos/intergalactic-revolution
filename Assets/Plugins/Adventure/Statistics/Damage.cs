/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2014-07-06 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Adventure.Statistics {

    /// Damage : IDamage
    /// Defines a low-level object to send statistical data
    public struct Damage {
        public float vitality {get;set;}
        public int Critical {get;set;}
        void Hit(int damage) => vitality -= damage;
    }
}
