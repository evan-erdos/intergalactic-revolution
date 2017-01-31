/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-24 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Locales {
    public class Room : Thing {
        new Collider collider;
        public override float Range => 0;
        public List<Thing> Things {get;} = new List<Thing>();

        protected override void Awake() { base.Awake();
            collider = GetOrAdd<Collider>();
            gameObject.layer = LayerMask.NameToLayer("Room");
            Location = transform.Find("location");
            Things.Add(GetComponentsInChildren<Thing>());
            collider.enabled = false;
        }

        new public class Data : Thing.Data {
            public List<Thing.Data> things {get;set;}
            public override BaseObject Deserialize(BaseObject o) {
                var instance = base.Deserialize(o) as Room;
                instance.collider.enabled = true;
                return instance;
            }
        }
    }
}
