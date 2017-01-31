/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-22 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Inventories {
    public class Item : Thing, IItem {
        new protected Rigidbody rigidbody;
        [SerializeField] protected StoryEvent onTake;
        [SerializeField] protected StoryEvent onDrop;
        [SerializeField] protected AudioClip sound;
        public event StoryAction TakeEvent, DropEvent;
        public bool Held {get;protected set;}
        public decimal? Cost {get;protected set;}
        public override float Range => 8;
        public float Mass {
            get { return rigidbody.mass; }
            set { rigidbody.mass = value; } }
        protected string MassName => $"<cmd>{Mass:N}kg</cmd>";
        public override string Name => $"{base.Name} : {MassName}";
        public override string Content => $"{base.Content}\n{CostName}";
        public virtual string CostName { get {
            if (Cost==null) return $" ";
            else if (Cost<0) return $"It's cursed. You can't sell it.";
            else if (Cost==0) return $"It's <cost>worthless</cost>.";
            else if (Cost==1) return $"It is worth <cost>{Cost} coin</cost>.";
            else return $"It is worth <cost>{Cost} coins</cost>."; } }

        public virtual void Use() => Drop();

        public virtual void Take() => TakeEvent(this, new StoryArgs {
            Input = $"take {Name}",
            Verb = new Verb(Description.Nouns, new[] { Name })});

        public virtual void Drop() => DropEvent(this, new StoryArgs {
            Input = $"drop {Name}",
            Verb = new Verb(Description.Nouns, new[] { Name })});

        public virtual void OnTake() {
            StartSemaphore(Taking);
            IEnumerator Taking() {
                transform.parent = Location;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                Log($"<cmd>The Monk takes the {base.Name}.</cmd>");
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                GetComponentsInChildren<Renderer>().ForEach(o => o.enabled = false);
                GetComponentsInChildren<Collider>().ForEach(o => o.enabled = false);
                Held = true;
                yield return new WaitForSeconds(1f);
            }
        }

        public virtual void OnDrop() {
            StartSemaphore(Dropping);
            IEnumerator Dropping() {
                AudioSource.PlayClipAtPoint(
                    clip: sound,
                    position: transform.position,
                    volume: 0.9f);
                rigidbody.AddForce(
                    force: Quaternion.identity.eulerAngles*4,
                    mode: ForceMode.VelocityChange);
                Log($"<cmd>The Monk drops the {base.Name}.</cmd>");
                rigidbody.isKinematic = false;
                rigidbody.useGravity = true;
                gameObject.SetActive(true);
                GetComponentsInChildren<Renderer>().ForEach(o => o.enabled = true);
                GetComponentsInChildren<Collider>().ForEach(o => o.enabled = true);
                Held = false;
                yield return new WaitForSeconds(1);
            }
        }

        protected override void Awake() { base.Awake();
            gameObject.layer = LayerMask.NameToLayer("Item");
            rigidbody = GetOrAdd<Rigidbody>();
            onTake.AddListener((o,e) => OnTake());
            onDrop.AddListener((o,e) => OnDrop());
            TakeEvent += (o,e) => onTake?.Invoke(o,e);
            DropEvent += (o,e) => onDrop?.Invoke(o,e);
        }

        new public class Data : Thing.Data {
            public decimal? cost {get;set;}
            public float mass {get;set;}
            public override BaseObject Deserialize(BaseObject o) {
                var instance = base.Deserialize(o) as Item;
                instance.Cost = this.cost;
                instance.Mass = this.mass;
                return instance;
            }
        }
    }
}
