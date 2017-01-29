/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-06 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Adventure {

    /// Word : interface
    /// any variety of recognizable, textual pattern
    public interface Word {

        /// Pattern : /regex/
        /// the pattern to be used to disambiguate objects
        Regex Pattern {get;}

        /// Grammar : string
        /// discrete options used for processing object definitions
        string[] Grammar {get;}
    }
}
