/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-11-02 */

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Adventure {

    /// RealAction : (real) => void
    /// handler for signaling any change in a number [0...1]
    public delegate void RealAction(float value);

    /// RealEvent : UnityEvent
    /// a serializable event handler to expose to the editor
    [Serializable] public class RealEvent : UnityEvent<float> { }

    public class Settings {
        public string Name {get;set;} = "Adventure";
        public string Date {get;set;} = "2017-01-01";
        public string Release {get;set;} = "v0.3.1";
        public string Author {get;set;} = "Ben Scott";
        public string Handle {get;set;} = "@evan-erdos";
        public string Email {get;set;} = "bescott@andrew.cmu.edu";
        public string Link {get;set;} = "bescott.org/adventure/";
    }
}
