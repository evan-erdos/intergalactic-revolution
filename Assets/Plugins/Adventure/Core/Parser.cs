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
        public event StoryAction LogEvent;
        public static Person player;

        public Parser(Map<Verb> verbs, StoryAction onLog) {
            var commands = new Map<StoryAction>() {
                ["quit"] = (o,e) => player.Do(o as Thing, e),
                ["redo"] = (o,e) => player.Do(o as Thing, e),
                ["save"] = (o,e) => player.Do(o as Thing, e),
                ["load"] = (o,e) => player.Do(o as Thing, e),
                ["help"] = (o,e) => player.Help(o as Thing, e),
                ["view"] = (o,e) => player.View(o as Thing, e),
                ["goto"] = (o,e) => player.Goto(o as Thing, e),
                ["take"] = (o,e) => player.Take(o as Thing, e),
                ["drop"] = (o,e) => player.Drop(o as Thing, e),
                ["use" ] = (o,e) => player.Use(o as Thing, e),
                ["wear"] = (o,e) => player.Wear(o as Thing, e),
                ["stow"] = (o,e) => player.Stow(o as Thing, e),
                ["open"] = (o,e) => player.Open(o as Thing, e),
                ["shut"] = (o,e) => player.Shut(o as Thing, e),
                ["push"] = (o,e) => player.Kill(o as Thing, e),
                ["pull"] = (o,e) => player.Pull(o as Thing, e),
                ["read"] = (o,e) => player.Read(o as Thing, e),
                ["pray"] = (o,e) => player.Pray(o as Thing, e),
                ["kill"] = (o,e) => player.Kill(o as Thing, e),
                ["sit" ] = (o,e) => player.Sit(o as Thing, e),
                ["stand"] = (o,e) => player.Stand(o as Thing, e),
                ["do"] = (o,e) => player.Do(o as Thing, e) };

            foreach (var verb in verbs)
                this.verbs[verb.Key] = new Verb(
                    pattern: verb.Value.Pattern,
                    grammar: verb.Value.Grammar,
                    command: commands[verb.Key]);

            LogEvent += (o,e) => onLog(o,e);

        }

        /// Process : (string) => string[]
        /// Input taken directly from the user cannot be used to
        /// issue Commands without being organized first. This
        /// function takes the raw input string, and returns a
        /// more rigorously organized structure.
        /// - input : string
        ///     raw user input from the Terminal
        public List<string> Process(string input) =>
            Process(new List<string>(input
                .Trim().ToLower()
                .Replace("\bthe\b","").Replace("\ba\b","")
                .Split('.'))).ToList();

        IEnumerable<string> Process(List<string> query) =>
            from elem in query
            where !string.IsNullOrEmpty(elem)
            select elem;

        public void Failure(string input, string s="") =>
            LogEvent(null, new StoryArgs($@"<cmd>{input}</cmd>: {s}".md()));

        /// Execute : (command, string) => void
        /// When a command is parsed in and evaluated, it is
        /// sent here, and a Command is created, dispatched to
        /// its function for processing, and in the case
        /// of a StoryError, it is resolved, such that
        /// an appropriate action might be taken. Any kind of
        /// text command Exception ends here, as they are used
        /// only for indicating errors in game logic, not errors
        /// relating to anything actually wrong with the code.
        /// - verb : Verb
        ///     the command struct without input
        /// - input : string
        ///     the raw, user-issued command
        /// - throw : StoryError
        ///     thrown when command is incoherent/malformed
        public bool Execute(Verb verb, string input) {
            try { verb.Command(player, new StoryArgs(verb, input)); return true; }
            catch (MoralityError error) { ResolveMorality(error); }
            catch (AmbiguityError error) { ResolveAmbiguity(error); }
            catch (StoryError error) { Resolve(error); }
            return false;

            void Resolve(StoryError error) => Failure(input, error.Message);
            void ResolveMorality(MoralityError error) {
                if (error.cond()) error.then(null, new StoryArgs()); }
            void ResolveAmbiguity(AmbiguityError error) {
                var sb = new StringBuilder();
                sb.AppendLine(error.Message);
                foreach (var elem in error.options)
                    sb.AppendLine($"<cmd>-</cmd> {elem.Name} ");
                LogEvent(null,new StoryArgs(Terminal.Format(sb.ToString().md())));
            }
        }

        /// Evaluate : (s) => bool
        /// Parses the sent string, creates a Command
        /// and dispatches it to its Parse function for processing.
        public void Evaluate(string lines) {
            Process(lines).ForEach(line => Eval(line));
            void Eval(string line) {
                if (string.IsNullOrEmpty(line.Trim())) return;
                var list =
                    from verb in verbs.Values
                    where verb.Pattern.IsMatch(line)
                    select verb;
                if (!list.Any()) Failure(lines, "You can't do that.");
                foreach (var item in list) if (Execute(item,line)) return;
            }
        }

        /// Resolve : (verb, T[]) => void
        /// When a verb is ambiguous or doesn't make any sense,
        /// this prompts the user for some explanation.
        public void Resolve<T>(Verb verb, List<T> list) =>
            LogEvent(null, new StoryArgs(list.Aggregate(
                "<cmd>Which do you mean</cmd>: ",
                (m,s) => m += $" <cmd>-</cmd> {s}").md()));
    }
}
