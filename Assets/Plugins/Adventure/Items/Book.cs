/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-11-04 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Inventories {
    public class Book : Item, IReadable {
        [SerializeField] protected Event<StoryArgs> onRead = new Event<StoryArgs>();
        public event AdventureAction<StoryArgs> ReadEvent {add{onRead.Add(value);} remove{onRead.Remove(value);}}
        public virtual string Passage {get;set;}
        public override void Drop(StoryArgs e=null) => Log(Description["attempt drop"]);
        public virtual void Read(StoryArgs e=null) => onRead?.Call(e ?? new StoryArgs { Sender = this });
        async void OnRead(StoryArgs e) { Log($"{Passage}"); await 2; }
        protected override void Awake() { base.Awake(); ReadEvent += e => OnRead(e); }

        new public class Data : Item.Data {
            public string Passage {get;set;}
            public override Object Deserialize(Object o) {
                var instance = base.Deserialize(o) as Book;
                instance.Passage = this.Passage;
                return instance;
            }
        }
    }
}
