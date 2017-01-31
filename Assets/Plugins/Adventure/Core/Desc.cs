/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-01-01 */

using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Adventure {

    /// Desc : ILoggable
    /// Deals with most of the functions of describable things.
    /// It stores a name, some adjectives and nouns which can be
    /// used to refer to stuff (by way of a Regex), along with
    /// some style options, and another string which serves as a
    /// formatting template for the other elements.
    public class Desc : ILoggable {
        Map<IEnumerator<string>> routines = new Map<IEnumerator<string>>();

        /// DefiniteArticle : string
        /// article to use to refer to something specifically
        public string DefiniteArticle {get;set;} = "the";

        /// IndefiniteArticle : string
        /// article to use to refer to something when it's not known
        public string IndefiniteArticle {get;set;} = "a";

        /// Name : string
        /// a plain old name, used when reading in descriptions from files
        public string Name {get;set;} = "object";

        /// Title : string
        /// the name, but with the proper tense and prepositions
        public string Title => $"{DefiniteArticle} {Name}";

        /// Template : string
        /// a string to be used to apply formatting to the Content
        public virtual string Template => $"### {Name} ###\n{Content}";

        /// Content : string
        /// the unprocessed, raw value of the description
        public string Content {get;set;} = "An object.";

        /// Nouns : /regex/
        /// A regex to be matched against user input, which
        /// acts as a dynamic identifier for the object.
        public Regex Nouns {get;set;} = new Regex("\b(object)\b");

        /// Responses : { string -> string[] }
        /// A mapping from string keys to lists of response text.
        public Map<List<string>> Responses {get;set;} = new Map<List<string>>();

        public Desc() : this(" ") { }
        internal Desc(string desc) : this(desc, new Regex(" ")) { }
        internal Desc(string desc, Regex nouns) { (Content, Nouns) = (desc, nouns); }

        public string this[string s] => FindDesc(s);
        string FindDesc(string s) {
            if (!Responses?.ContainsKey(s)==true) return "";
            if (!routines.ContainsKey(s)) routines[s] = Responses?[s]?.GetEnumerator();
            var enumerator = routines[s];
            if (!enumerator.MoveNext()) return enumerator.Current;
            return enumerator.Current;
        }

        public bool Fits(string o) => !string.IsNullOrEmpty(Nouns.ToString())
            && !string.IsNullOrEmpty(o) && (Nouns.IsMatch(o) || o==Name);
        public void Log() => Terminal.Log(Content.md());
        public override string ToString() => Content.md();
        public static implicit operator string(Desc o) => o.Content;
    }
}
