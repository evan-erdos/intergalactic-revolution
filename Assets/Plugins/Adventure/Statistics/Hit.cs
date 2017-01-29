/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-11-13 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Adventure.Statistics {

    /// Hit : struct
    /// Low-level struct to represent a Hit. Factors in the
    /// damage rolls, the resistances, & its own affinity.
    public struct Hit {
        public int value;
        public Damages damage;
        public Affinities affinity;
        public Hit(
                int value=0,
                Damages damage=Damages.Default,
                Affinities affinity=Affinities.Miss) {
            (this.value, this.damage, this.affinity) = (value, damage, affinity); }
        public override string ToString() => $"~{value}:|{affinity}|~";
    }
}
