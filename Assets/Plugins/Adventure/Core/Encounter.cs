/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-07-23 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure {
    public class Encounter : Thing {
        new Collider collider;
        public bool IsReusable {get;protected set;}
        public float InitialDelay {get;protected set;}

        async void Start() {
            collider = GetOrAdd<Collider,BoxCollider>();
            if (InitialDelay<0) { await InitialDelay; Begin(); }
        }

        async void Begin() {
            Log(Description);
            if (IsReusable) {
                if (collider) collider.enabled = false;
                await 1;
                if (collider) collider.enabled = true;
            } else gameObject.SetActive(false);
        }

        void OnTriggerEnter(Collider o) { if (o.GetParent<Player>() is Player player) Begin(); }

        new public class Data : Thing.Data {
            public bool isTimed {get;set;}
            public bool reuse {get;set;}
            public float initialDelay {get;set;}
            public override Object Deserialize(Object o) {
                var instance = base.Deserialize(o) as Encounter;
                instance.IsReusable = this.reuse;
                instance.InitialDelay = this.initialDelay;
                return instance;
            }
        }
    }
}
