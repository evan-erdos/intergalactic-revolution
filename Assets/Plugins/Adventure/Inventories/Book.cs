/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-11-04 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Inventories {
    public class Book : Item, IReadable {
        YieldInstruction wait = new WaitForSeconds(2);
        [SerializeField] protected StoryEvent onRead;
        public event StoryAction ReadEvent;
        public virtual string Passage {get;set;}
        public override void Drop() => Log(Description["attempt drop"]);
        public virtual void Read() => ReadEvent(this, new StoryArgs());

        public void OnRead() { StartSemaphore(Reading);
            IEnumerator Reading() { Log($"{Passage}"); yield return wait; } }

        protected override void Awake() { base.Awake();
            onRead.AddListener((o,e) => OnRead());
            ReadEvent += (o,e) => onRead?.Invoke(o,e);
        }

        new public class Data : Item.Data {
            public string Passage {get;set;}
            public override BaseObject Deserialize(BaseObject o) {
                var instance = base.Deserialize(o) as Book;
                instance.Passage = this.Passage;
                return instance;
            }
        }
    }
}
