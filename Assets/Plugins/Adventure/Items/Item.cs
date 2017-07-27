/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-22 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Inventories {
    public class Item : Thing, IItem {
        new protected Rigidbody rigidbody;
        [SerializeField] protected AudioClip sound;
        [SerializeField] protected Event<StoryArgs> onTake = new Event<StoryArgs>();
        [SerializeField] protected Event<StoryArgs> onDrop = new Event<StoryArgs>();
        public event AdventureAction<StoryArgs> TakeEvent {add{onTake.Add(value);} remove{onTake.Remove(value);}}
        public event AdventureAction<StoryArgs> DropEvent {add{onDrop.Add(value);} remove{onDrop.Remove(value);}}
        public bool Held {get;protected set;}
        public decimal? Cost {get;protected set;}
        public override float Radius => 8;
        public float Mass { get { return rigidbody.mass; } set { rigidbody.mass = value; } }
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

        public virtual void Take(StoryArgs e=null) => onTake?.Call(e ?? new StoryArgs {
            Sender = this, Input = $"take {Name}", Verb = new Verb(Description.Nouns, new[] { Name })});

        public virtual void Drop(StoryArgs e=null) => onDrop?.Call(e ?? new StoryArgs {
            Sender = this, Input = $"drop {Name}", Verb = new Verb(Description.Nouns, new[] { Name })});

        public virtual void OnTake() {
            StartSemaphore(Taking);
            IEnumerator Taking() {
                transform.parent = Location;
                (transform.localPosition, transform.localRotation) = (Vector3.zero,Quaternion.identity);
                Log($"<cmd>The Monk takes the {base.Name}.</cmd>");
                (rigidbody.isKinematic, rigidbody.useGravity) = (true,false);
                GetChildren<Renderer>().ForEach(o => o.enabled = false);
                GetChildren<Collider>().ForEach(o => o.enabled = false);
                Held = true;
                yield return new WaitForSeconds(1);
            }
        }

        public virtual void OnDrop() {
            StartSemaphore(Dropping);
            IEnumerator Dropping() {
                AudioSource.PlayClipAtPoint(sound, transform.position, 0.9f);
                rigidbody.AddForce(Quaternion.identity.eulerAngles*4, ForceMode.VelocityChange);
                Log($"<cmd>The Monk drops the {base.Name}.</cmd>");
                (rigidbody.isKinematic, rigidbody.useGravity) = (false, true);
                gameObject.SetActive(true);
                GetChildren<Renderer>().ForEach(o => o.enabled = true);
                GetChildren<Collider>().ForEach(o => o.enabled = true);
                Held = false;
                yield return new WaitForSeconds(1);
            }
        }

        protected override void Awake() { base.Awake();
            gameObject.layer = LayerMask.NameToLayer("Item");
            rigidbody = GetOrAdd<Rigidbody>();
            TakeEvent += e => OnTake();
            DropEvent += e => OnDrop();
        }

        new public class Data : Thing.Data {
            public decimal? cost {get;set;}
            public float mass {get;set;}
            public override Object Deserialize(Object o) {
                var instance = base.Deserialize(o) as Item;
                instance.Cost = this.cost;
                instance.Mass = this.mass;
                return instance;
            }
        }
    }
}
