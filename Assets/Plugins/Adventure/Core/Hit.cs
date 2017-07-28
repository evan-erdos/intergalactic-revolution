/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-11-13 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Adventure.Statistics {

    /// Hit : struct
    /// represents a hit, Factors in damage rolls, resistances, & crit
    public struct Hit {
        public int value {get;set;} public Damages damage {get;set;} public Affinities crit {get;set;}
        public Hit(int value=0, Damages damage=Damages.Default, Affinities crit=Affinities.Miss) {
            (this.value, this.damage, this.crit) = (value, damage, crit); }
        public override string ToString() => $"~{value}:|{crit}|~";
    }
}
