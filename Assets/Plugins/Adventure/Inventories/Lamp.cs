/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-13 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Inventories {
    public class Lamp : Item, IWieldable {
        Animator animator;
        List<Light> lights = new List<Light>();
        List<Renderer> renderers = new List<Renderer>();
        public bool IsLit {get;protected set;}
        public bool Worn {get;protected set;}
        public bool Used {get;set;}
        public float Time {get;set;}
        public virtual string TimeName => $"<cost>({Time}min)</cost>";
        public override string Name => $"{base.Name} {TimeName}";
        public Transform Grip {get;protected set;}
        public override void Use() => Ignite();
        public void Attack() => Use();

        void Ignite() {
            if (!IsLit && gameObject.activeInHierarchy && animator)
                StartCoroutine(Igniting());
            IEnumerator Igniting() {
                StartCoroutine(LateSetBool("on",IsLit));
                lights.ForEach(light => light.enabled = true);
                while (0<Time--) yield return new WaitForSeconds(60);
                lights.ForEach(light => light.enabled = false);
            }
        }

        IEnumerator LateSetBool(string s, bool t) {
            yield return new WaitForEndOfFrame();
            if (animator && animator.enabled) animator.SetBool(s,t);
        }

        public override void Take() { base.Take();
            renderers.ForEach(o => o.enabled = true); Wear(); }

        public virtual void Wear() {
            Worn = true;
            if (gameObject.activeInHierarchy)
                StartCoroutine(LateSetBool("worn",Worn));
            IsLit = true;
        }

        public virtual void Stow() {
            Worn = false;
            if (gameObject.activeInHierarchy)
                StartCoroutine(LateSetBool("worn",Worn));
            gameObject.SetActive(false);
        }

        protected override void Awake() { base.Awake();
            animator = GetComponent<Animator>();
            lights.AddRange(GetComponentsInChildren<Light>());
            renderers.AddRange(GetComponentsInChildren<Renderer>());
            Grip = GetOrAdd("grip");
            Grip.parent = transform;
        }

        new public class Data : Item.Data {
            public bool lit {get;set;}
            public float time {get;set;}
            public override BaseObject Deserialize(BaseObject o) {
                var instance = base.Deserialize(o) as Lamp;
                instance.IsLit = this.lit;
                instance.Time = this.time;
                return instance;
            }
        }
    }
}
