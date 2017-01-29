/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-12-05 */

using System.Text.RegularExpressions;

namespace Adventure {

    /// Message : ILoggable
    /// Represents any textual message that can be formatted and rendered
    public struct Message : ILoggable {
        public string Name {get;set;}
        public string Content {get;set;}

        public Message(string content="", string name="") : this() {
            (this.Name, this.Content) = (name, content); }

        public void Log() => Terminal.Log(Content.md());
        public override string ToString() => Content;
        public static implicit operator string(Message o) => o.Content.md();
    }
}
