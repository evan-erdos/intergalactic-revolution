/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-11-11 */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Adventure {


    /// Parser
    /// Main class for dealing with natural language commands,
    /// the verbs that can be used on their particular "nouns",
    /// and all manner of other user-issued commands.
    public class Parser {
        Map<Verb> verbs = new Map<Verb>();
        public event AdventureAction<StoryArgs> LogEvent;
        public static Person player;

        public Parser(Map<Verb> verbs, AdventureAction<StoryArgs> onLog) {
            var commands = new Map<AdventureAction<StoryArgs>>() {
                ["quit"] = e => player.Do(e), ["redo"] = e => player.Do(e),
                ["save"] = e => player.Do(e), ["load"] = e => player.Do(e),
                ["help"] = e => player.Help(e), ["view"] = e => player.View(e),
                ["goto"] = e => player.Goto(e), ["take"] = e => player.Take(e),
                ["drop"] = e => player.Drop(e), ["use" ] = e => player.Use(e),
                ["wear"] = e => player.Wear(e), ["stow"] = e => player.Stow(e),
                ["open"] = e => player.Open(e), ["shut"] = e => player.Shut(e),
                ["push"] = e => player.Kill(e), ["pull"] = e => player.Pull(e),
                ["read"] = e => player.Read(e), ["pray"] = e => player.Pray(e),
                ["kill"] = e => player.Kill(e), ["sit" ] = e => player.Sit(e),
                ["stand"] = e => player.Stand(e), ["do"] = e => player.Do(e) };

            foreach (var verb in verbs) this.verbs[verb.Key] = new Verb(
                verb.Value.Pattern, verb.Value.Grammar, commands[verb.Key]);
            LogEvent += e => onLog(e);
        }


        /// Process : (string) => string[]
        /// input taken directly from the user
        public List<string> Process(string input) => Process(new List<string>(input
            .Trim().ToLower().Replace("\bthe\b","").Replace("\ba\b","").Split('.'))).ToList();

        IEnumerable<string> Process(List<string> query) =>
            from elem in query where !string.IsNullOrEmpty(elem) select elem;

        public void Failure(string input, string s="") => LogEvent(new StoryArgs($@"<cmd>{input}</cmd>: {s}".md()));

        /// Execute : (command, string) => void
        /// When a command is parsed in and evaluated, it is
        /// sent here, and a Command is created, dispatched to
        /// its function for processing, and in the case
        /// of a StoryError, it is resolved, such that
        /// an appropriate action might be taken. Any kind of
        /// text command Exception ends here, as they are used
        /// only for indicating errors in game logic, not errors
        /// relating to anything actually wrong with the code.
        /// - verb : the command struct without input
        /// - input : the raw, user-issued command
        /// - throw : StoryError thrown when command is incoherent/malformed
        public bool Execute(Verb verb, string input) {
            try { verb.Command(new StoryArgs { Sender = player, Verb = verb, Input = input }); return true; }
            catch (MoralityError e) { ResolveMorality(e); }
            catch (AmbiguityError e) { ResolveAmbiguity(e); }
            catch (StoryError e) { Resolve(e); }
            return false;

            void Resolve(StoryError e) => Failure(input, e.Message);
            void ResolveMorality(MoralityError e) { if (e.cond()) e.then(new StoryArgs()); }
            void ResolveAmbiguity(AmbiguityError e) {
                var sb = new StringBuilder(e.Message);
                foreach (var o in e.options) sb.AppendLine($"<cmd>-</cmd> {o.Name} ");
                LogEvent(new StoryArgs(Terminal.Format(sb.ToString().md())));
            }
        }


        /// Evaluate : (s) => bool
        /// Parses the sent string, creates a Command
        /// and dispatches it to its Parse function for processing.
        public void Evaluate(string lines) {
            Process(lines).ForEach(o => Eval(o));
            void Eval(string s) {
                if (string.IsNullOrEmpty(s.Trim())) return;
                var list = from verb in verbs.Values where verb.Pattern.IsMatch(s) select verb;
                if (!list.Any()) Failure(lines, "You can't do that.");
                foreach (var item in list) if (Execute(item,s)) return;
            }
        }


        /// Resolve : (verb, T[]) => void
        /// When a verb is ambiguous or doesn't make any sense,
        /// this prompts the user for some explanation.
        public void Resolve<T>(Verb verb, List<T> list) => LogEvent(new StoryArgs(
            list.Aggregate("<cmd>Which do you mean</cmd>: ", (m,s) => m += $" <cmd>-</cmd> {s}").md()));
    }
}
