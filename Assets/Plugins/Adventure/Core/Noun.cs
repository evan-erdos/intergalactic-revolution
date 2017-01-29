/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-06 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Adventure {

    /// Noun : Word
    /// a kind of word which refers to a thing which can be acted on
    public struct Noun : Word {
        public Regex Pattern {get;set;}
        public string[] Grammar {get;set;}
        public Noun(Regex pattern, string[] grammar) {
            (this.Pattern, this.Grammar) = (pattern, grammar);
        }
    }
}
