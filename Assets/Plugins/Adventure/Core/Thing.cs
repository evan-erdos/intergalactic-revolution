/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-22 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure {
    public class Thing : Object, IThing {
        [SerializeField] protected StoryEvent onLog = new StoryEvent();
        [SerializeField] protected StoryEvent onView = new StoryEvent();
        [SerializeField] protected StoryEvent onFind = new StoryEvent();
        [SerializeField] protected StoryEvent onTouch = new StoryEvent();
        public event AdventureAction<StoryArgs> LogEvent, ViewEvent, FindEvent, TouchEvent;
        public override float Radius => 5;
        public override string Name => $"**{name}**";
        public virtual string Content => $"### {Name} ###\n{Description}";
        public virtual Desc Description {get;protected set;} = new Desc();
        public virtual string this[string o] { get { return Description[o]; } }
        public virtual void Do() => Touch();
        public virtual void Log() => Log(Content);
        public virtual void Log(string s) => LogEvent(new StoryArgs(s));
        public virtual void Find(StoryArgs e=null) => FindEvent(e ?? new StoryArgs { Sender=this });
        public virtual void View(StoryArgs e=null) => ViewEvent(e ?? new StoryArgs { Sender=this });
        public virtual void Touch(StoryArgs e=null) => TouchEvent(e ?? new StoryArgs { Sender=this });
        public override bool Fits(string s) => Description.Fits(s);
        public virtual void OnLog(string s) => Terminal.Log(s.md());
        async Task OnView() { Terminal.Log(Content.md()); await 1; }
        async Task OnFind() { Terminal.Log(Description["find"].md()); await 1; }
        async Task OnTouch() { await 1; }

        protected virtual void Awake() {
            gameObject.layer = LayerMask.NameToLayer("Thing");
            Mask =
                  1 << LayerMask.NameToLayer("Thing")
                | 1 << LayerMask.NameToLayer("Item")
                | 1 << LayerMask.NameToLayer("Room")
                | 1 << LayerMask.NameToLayer("Actor");
            LogEvent += e => onLog?.Call(e); onLog.Add(e => OnLog(e.Message));
            FindEvent += e => onFind?.Call(e); onFind.Add(e => StartAsync(OnFind));
            ViewEvent += e => onView?.Call(e); onView.Add(e => StartAsync(OnView));
            TouchEvent += e => onTouch?.Call(e); onTouch.Add(e => StartAsync(OnTouch));
        }

        void OnCollision(Collision o) => If(o.rigidbody.tag=="Player", Do);

        new public class Data : Object.Data {
            public Desc description {get;set;}
            public Map<List<string>> responses {get;set;}

            public override void Merge(Object.Data o) { base.Merge(o);
                if (o is Thing.Data d && d.responses!=null)
                    foreach (var r in d.responses) responses[r.Key] = r.Value; }

            public override Object Deserialize(Object o) {
                var instance = base.Deserialize(o) as Thing;
                instance.Description.Name = instance.name;
                if (description!=null) {
                    instance.Description.Nouns = description.Nouns;
                    instance.Description.Content = description.Content; }
                if (responses!=null) foreach (var r in responses)
                    instance.Description.Responses[r.Key] = r.Value;
                instance.enabled = true;
                return o;
            }
        }
    }
}
