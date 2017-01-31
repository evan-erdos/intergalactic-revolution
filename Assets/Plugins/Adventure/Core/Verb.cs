/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-06 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Adventure {

    /// Verb : Word
    /// any sort of word which correlates to a command
    public struct Verb : Word {
        public Regex Pattern {get;set;}
        public string[] Grammar {get;set;}
        public StoryAction Command {get;set;}
        public Verb(Regex pattern, string[] grammar, StoryAction command=null) {
            (this.Pattern, this.Grammar, this.Command) = (pattern, grammar, command); }
    }
}
