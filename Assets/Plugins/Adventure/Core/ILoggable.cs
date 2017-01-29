/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-06 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Adventure {

    /// ILoggable : interface
    /// anything which can be logged, descriptions, messages, etc
    public interface ILoggable {

        /// Content : string
        /// unformatted representation of this instance's description
        string Content {get;}

        /// Log : () => void
        /// event callback, returns the fully-formatted string to be displayed
        void Log();
    }
}
