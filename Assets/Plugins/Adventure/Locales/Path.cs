/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-01-01 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Locales {
    public class Path : Thing, IPath { // where T : Room where U : Room
        [SerializeField] protected StoryEvent onTravel;
        public event StoryAction TravelEvent;
        public Room Destination {get;protected set;}
        protected virtual string PathText => $"It leads to {Destination}.";
        public override string Content => $"{base.Content}\n{PathText}.";
        public virtual void Travel(Thing o) => TravelEvent(o,new StoryArgs());

        void OnTravel(Thing thing) {
            StartSemaphore(Traveling);
            IEnumerator Traveling() {
                Log($"{thing.Name} travels to {Destination}");
                yield return new WaitForSeconds(1);
            }
        }

        protected override void Awake() { base.Awake();
            TravelEvent += (o,e) => onTravel?.Invoke(o,e); }

        new public class Data : Thing.Data {
            public string destination = "Cloister";
            public override BaseObject Deserialize(BaseObject o) {
                var instance = base.Deserialize(o) as Path;
                instance.Destination = Story.Rooms[destination];
                return instance;
            }
        }
    }
}
