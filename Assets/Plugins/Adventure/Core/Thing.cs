/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-22 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure {
    public class Thing : Object, IThing {
        [SerializeField] protected StoryEvent onLog, onView, onFind, onTouch;
        public event StoryAction LogEvent, ViewEvent, FindEvent, TouchEvent;
        public override float Radius => 5;
        public override string Name => $"**{name}**";
        public virtual string Content => $"### {Name} ###\n{Description}";
        public virtual Transform Location {get;set;}
        public virtual Desc Description {get;protected set;} = new Desc();
        public virtual string this[string o] { get { return Description[o]; } }
        public virtual void Do() => Touch();
        public virtual void Log() => Log(Content);
        public virtual void Log(string s) => LogEvent(null,new StoryArgs(s));
        public virtual void Find() => FindEvent(this,new StoryArgs());
        public virtual void View() => ViewEvent(this,new StoryArgs());
        public virtual void Touch() => TouchEvent(this,new StoryArgs());
        public override bool Fits(string s) => Description.Fits(s);
        public virtual void OnLog(string s) => Terminal.Log(s.md());
        async Task OnView() { Terminal.Log(Content.md()); await 1; }
        async Task OnFind() { Terminal.Log(Description["find"].md()); await 1; }

        protected virtual void Awake() {
            Mask =
                  1 << LayerMask.NameToLayer("Thing")
                | 1 << LayerMask.NameToLayer("Item")
                | 1 << LayerMask.NameToLayer("Room")
                | 1 << LayerMask.NameToLayer("Actor");
            gameObject.layer = LayerMask.NameToLayer("Thing");
            onLog.AddListener((o,e) => OnLog(e.Message));
            onFind.AddListener((o,e) => StartAsync(OnFind));
            onView.AddListener((o,e) => StartAsync(OnView));
            LogEvent += (o,e) => onLog?.Invoke(o,e);
            FindEvent += (o,e) => onFind?.Invoke(o,e);
            ViewEvent += (o,e) => onView?.Invoke(o,e);
            TouchEvent += (o,e) => onTouch?.Invoke(o,e);
        }

        void OnCollision(Collision o) => If(o.rigidbody.tag=="Player", Do);

        new public class Data : Object.Data {
            public Desc description {get;set;}
            public Map<List<string>> responses {get;set;}

            public override void Merge(Object.Data o) { base.Merge(o);
                var root = o as Thing.Data;
                if (root.responses!=null)
                    foreach (var pair in root.responses)
                        responses[pair.Key] = pair.Value; }

            public override Object Deserialize(Object o) {
                var instance = base.Deserialize(o) as Thing;
                instance.Description.Name = instance.name;
                if (description!=null) {
                    instance.Description.Nouns = description.Nouns;
                    instance.Description.Content = description.Content; }
                if (responses!=null) foreach (var pair in responses)
                    instance.Description.Responses[pair.Key] = pair.Value;
                instance.enabled = true;
                return o;
            }
        }
    }
}
