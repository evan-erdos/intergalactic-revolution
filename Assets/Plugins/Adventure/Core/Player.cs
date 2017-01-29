/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-07-12 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using ui=UnityEngine.UI;
using Adventure.Inventories;
using Adventure.Locales;

namespace Adventure {
    public class Player : Person {

        protected override void Awake() { base.Awake();
            gameObject.layer = LayerMask.NameToLayer("Player");
            Parser.player = this;
        }

        void OnTriggerEnter(Collider other) {
            var room = other.GetComponentInParent<Room>();
            if (!room || this.Location==room) return;
            this.Location = room.transform;
            room.View();
        }

        new public class Data : Person.Data {
            public override BaseObject Deserialize(BaseObject o) {
                var instance = base.Deserialize(o) as Player;
                return instance;
            }
        }
    }
}
