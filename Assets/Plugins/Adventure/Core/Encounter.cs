/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-07-23 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure {
    public class Encounter : Thing {
        new Collider collider;
        public bool IsReusable {get;protected set;}
        public float InitialDelay {get;protected set;}

        IEnumerator Start() {
            collider = GetOrAdd<Collider>();
            if (0>=InitialDelay) yield break;
            yield return new WaitForSeconds(InitialDelay);
            Begin();
        }

        void OnTriggerEnter(Collider o) => Begin();

        void Begin() {
            StartSemaphore(Beginning);
            IEnumerator Beginning() {
                Log(Description);
                if (IsReusable) {
                    if (collider) collider.enabled = false;
                    yield return new WaitForSeconds(1f);
                    if (collider) collider.enabled = true;
                } else gameObject.SetActive(false);
            }
        }

        new public class Data : Thing.Data {
            public bool isTimed {get;set;}
            public bool reuse {get;set;}
            public float initialDelay {get;set;}
            public override BaseObject Deserialize(BaseObject o) {
                var instance = base.Deserialize(o) as Encounter;
                instance.IsReusable = this.reuse;
                instance.InitialDelay = this.initialDelay;
                return instance;
            }
        }
    }
}
