/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-06 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure {

    /// StoryArgs : EventArgs
    /// encapsulates the event data
    /// - verb : Verb
    ///     Default Command struct, makes this function a StoryAction
    /// - input : string
    ///     raw input from the user
    /// - goal : IThing
    ///     specified target of action
    public class StoryArgs : System.EventArgs, Word {
        StoryAction command {get;set;}
        public Verb verb {get;set;}
        public Regex Pattern => verb.Pattern;
        public string[] Grammar => verb.Grammar;
        public string input {get;set;}
        public string message {get;set;}
        public IThing Goal {get;set;}

        public StoryArgs() : base() { }
        public StoryArgs(string message) { this.message = message; }
        public StoryArgs(IThing goal) : this("", goal) { }
        public StoryArgs(string input, IThing goal)
            : this(verb: new Verb(), input: input, message: "", goal: goal) { }
        public StoryArgs(
                        string pattern="(thing|object)",
                        string[] grammar=null,
                        string input="",
                        string message="",
                        StoryAction command=null,
                        IThing goal=null) : this(
            verb: new Verb(new Regex(pattern), grammar, command),
            input: input,
            message: message,
            goal: goal) { }

        public StoryArgs(
                        Verb verb,
                        string input="",
                        string message="",
                        IThing goal=null) {
            (this.input, this.message) = (input, message);
            (this.verb, this.Goal) = (verb, goal);
        }
    }
}
