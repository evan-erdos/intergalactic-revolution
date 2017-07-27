/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-01-01 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Locales {
    public class Path : Thing, IPath {
        [SerializeField] protected Event<TravelArgs> onTravel = new Event<TravelArgs>();
        public event AdventureAction<TravelArgs> TravelEvent {add{onTravel.Add(value);} remove{onTravel.Remove(value);}}
        public Room Destination {get;protected set;}
        protected virtual string PathText => $"It leads to {Destination}.";
        public override string Content => $"{base.Content}\n{PathText}.";
        public virtual void Travel(TravelArgs e=null) => onTravel?.Call(e ?? new TravelArgs { Sender = this });
        void OnTravel(TravelArgs e) => Log($"{e.Sender.Name} travels to {Destination}");
        protected override void Awake() { base.Awake(); TravelEvent += e => OnTravel(e); }

        new public class Data : Thing.Data {
            public string destination = "Cloister";
            public override Object Deserialize(Object o) {
                var instance = base.Deserialize(o) as Path;
                instance.Destination = Story.Rooms[destination];
                return instance;
            }
        }
    }
}
