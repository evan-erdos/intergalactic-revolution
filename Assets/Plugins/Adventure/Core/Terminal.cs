/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-07-11 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using ui=UnityEngine.UI;

namespace Adventure {
    public class Terminal : BaseObject {
        bool isLocked;
        float time = 0.5f, initTime = 10;
        Coroutine coroutine;
        Parser parser;
        Queue<string> logs = new Queue<string>();
        ui::Text log;
        ui::InputField input;
        static Queue<string> queue = new Queue<string>();
        public static Map<Verb> Verbs = new Map<Verb>();
        public static Map<Message> Messages = new Map<Message>();
        public event StoryAction LogEvent;
        public void Clear() { logs.Enqueue(log.text); log.text = ""; }
        public void LogMessage(string o) => Log(Messages[o]);
        public void Log(string o, bool f) => LogEvent(null, new StoryArgs(o));
        public static void Log(params string[] a) =>
            a.ForEach(o => queue.Enqueue(Format(o)));
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
                    message.Split('\n').ToList().Aggregate("",
                        (s,l) => s += $"\n    {l}");
                    break;
            } return message;
        }

        IEnumerator Logging() {
            while (true) {
                if (0<queue.Count && !isLocked) OnLog(queue.Dequeue());
                yield return new WaitForSeconds(time);
            }
        }

        void OnEnable() => LogEvent += (o,e) => Log(e.Message);
        void OnDisable() => LogEvent += (o,e) => Log(e.Message);

        void Awake() {
            input = GetComponentInChildren<ui::InputField>();
            log = GetComponentInChildren<ui::Text>();
            parser = new Parser(Verbs, (o,e) => Log(e.Message));
        }

        void Start() {
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
                var last = transform.position;
                var position = transform.position;
                var range = 100f;
                var mask =
                      1 << LayerMask.NameToLayer("Thing")
                    | 1 << LayerMask.NameToLayer("Item")
                    | 1 << LayerMask.NameToLayer("Room")
                    | 1 << LayerMask.NameToLayer("Actor");
                while (true) {
                    yield return new WaitForSeconds(1f);
                    if (transform.IsNear(last, range/9)) continue;
                    last = transform.position;
                    var query =
                        from collider in Physics.OverlapSphere(position,range,mask)
                        let instance = collider.GetComponentInParent<Thing>()
                        where instance!=null
                        select instance as Thing;
                    query.ToList().ForEach(thing => {
                        thing.LogEvent -= (o,e) => Log(e.Message);
                        thing.LogEvent += (o,e) => Log(e.Message); });
                }
            }
        }

        /// Styles : enum
        /// This enumerates the various formatting options that the
        /// Terminal can use. Most values have some meaning, which
        /// are used by the formatting function. They might
        /// be hex values for colors, sizes of headers, etc.
        public enum Styles {
            Inline=0, Newline=1, Paragraph=2, Refresh=3, Indent=4,
            h1=24, h2=18, h3=16, h4=14,
            Default=0xFFFFFF, State=0x2A98AA, Change=0xFFAE10,
            Alert=0xFC0000, Command=0xBBBBBB, Warning=0xFA2363,
            Help=0x9CDF91, Title=0x98C8FC, Static=0xFFDBBB
        }
    }
}
