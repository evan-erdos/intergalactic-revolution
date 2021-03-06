/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-07-11 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using ui=UnityEngine.UI;

namespace Adventure {
    public class Terminal : Object {
        bool isLocked;
        float time = 0.5f, initTime = 10;
        Coroutine coroutine;
        Parser parser;
        ui::Text log;
        ui::InputField input;
        Queue<string> logs = new Queue<string>();
        static Queue<string> queue = new Queue<string>();
        public static Map<Verb> Verbs = new Map<Verb>();
        public static Map<Message> Messages = new Map<Message>();
        public event AdventureAction<StoryArgs> LogEvent;
        public void Clear() { logs.Enqueue(log.text); log.text = ""; }
        public void LogMessage(string o) => Log(Messages[o]);
        public void Log(string o, bool f) => LogEvent(new StoryArgs(o));
        public static void Log(params string[] a) => a.ForEach(o => queue.Enqueue(Format(o)));
        public void CommandInput() => CommandInput(input.text);
        public void CommandInput(string o) {
            if (coroutine!=null) StopCoroutine(coroutine);
            if (coroutine!=null) isLocked = false;
            input.text = "";
            input.interactable = true;
            input.ActivateInputField();
            input.Select();
            parser.Evaluate(o.Trim());
        }

        void OnLog(string message) {
            StartSemaphore(FadeText);
            IEnumerator FadeText() {
                log.CrossFadeAlpha(0,0.01f,false);
                yield return new WaitForSeconds(0.01f);
                log.text = message;
                logs.Enqueue(message);
                log.CrossFadeAlpha(1,0.01f,false);
                yield return new WaitForSeconds(0.01f);
            }
        }

        public static string Format(string message, params Styles[] styles) {
            if (string.IsNullOrEmpty(message) || styles==null) return message;
            message = message.Trim();
            foreach (var elem in styles) switch (elem) {
                case Styles.Command: case Styles.State:
                case Styles.Change: case Styles.Alert:
                    message = $"<color=#{(int) elem:X}>{message}</color>"; break; }
            foreach (var elem in styles) switch (elem) {
                case Styles.h1: case Styles.h2:
                case Styles.h3: case Styles.h4:
                    message = $"<size={elem}>{message}</size>";
                    message = $"<color=#{(int) Styles.Title:X}>{message}</color>";
                    break;
                case Styles.Inline: message = message.Trim(); break;
                case Styles.Paragraph: message = $"\n\n{message}"; break;
                case Styles.Newline: message = $"\n{message}"; break;
                case Styles.Indent:
                    message.Split('\n').ToList().Aggregate("",(s,l) => s += $"\n    {l}");
                    break;
            } return message;
        }


        void OnEnable() => LogEvent += e => Log(e.Message);
        void OnDisable() => LogEvent += e => Log(e.Message);

        void Start() {
            (input,log) = (GetChild<ui::InputField>(), GetChild<ui::Text>());
            parser = new Parser(Verbs, e => Log(e.Message));
            input.interactable = true;
            input.ActivateInputField();
            input.Select();
            StopAllCoroutines();
            coroutine = StartCoroutine(Initializing());
            IEnumerator Initializing() {
                Clear();
                StartCoroutine(Logging());
                isLocked = true;
                OnLog(Messages["prologue"]);
                yield return new WaitForSeconds(initTime);
                isLocked = false;
                var (last,position) = (transform.position, transform.position);
                var range = 100f;
                var mask =
                      1 << LayerMask.NameToLayer("Thing")
                    | 1 << LayerMask.NameToLayer("Item")
                    | 1 << LayerMask.NameToLayer("Room")
                    | 1 << LayerMask.NameToLayer("Actor");
                while (true) {
                    yield return new WaitForSeconds(1);
                    if (transform.IsNear(last, range/9)) continue;
                    last = transform.position;
                    var query =
                        from collider in Physics.OverlapSphere(position,range,mask)
                        let instance = collider.GetParent<Thing>()
                        where instance!=null select instance as Thing;
                    query.ToList().ForEach(o => o.LogEvent += e => Log(e.Message));
                }

                IEnumerator Logging() {
                    while (true) {
                        if (0<queue.Count && !isLocked) OnLog(queue.Dequeue());
                        yield return new WaitForSeconds(time);
                    }
                }
            }
        }
    }
}
