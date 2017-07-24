/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-31 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Adventure.Inventories;
using Adventure.Locales;

namespace Adventure {
    public class Actor : Thing, IActor {
        new protected Rigidbody rigidbody;
        [SerializeField] protected StoryEvent onKill = new StoryEvent();
        [SerializeField] protected StoryEvent onGoto = new StoryEvent();
        public event StoryAction KillEvent, GotoEvent;
        public virtual bool IsDead {get;set;}
        public override float Radius => 16;
        public virtual float Mass => rigidbody.mass;
        public virtual decimal Health {get;set;} = 120;
        public virtual decimal Vitality {get;set;} = 128;
        public virtual string MassName => $"<cmd>{Mass:N}kg</cmd>";
        public virtual string LifeName => $"<life>({Health}/{Vitality})</life>";
        public override string Name => $"{base.Name} : {LifeName}, {MassName}";
        public override string Content => $"### {Name} ###\n{Description}";
        public virtual List<Item> Items {get;set;} = new List<Item>();
        public virtual Transform WalkTarget {get;protected set;}
        public virtual Transform LookTarget {get;protected set;}
        public override Transform Location {set {
            if (value.GetParent<Room>() is Room o) base.Location = o.Location;
            else throw new StoryError(Description["cannot goto"]); } }

        public virtual void Kill(Actor o) => KillEvent(this,
            args: new StoryArgs { Message = $"kill {o}", Goal = o });
        public virtual void Goto(IThing o) => GotoEvent(this,
            args: new StoryArgs { Message = $"go to {o}", Goal = o });
        public override void Do() => Talk();
        public virtual void Take() => Find<Item>().ForEach(o => Take(o));
        public virtual void Drop() => Items.ForEach(item => Drop(item as Thing));
        public virtual void Talk() => Log(Description["talk"]);
        public virtual void Help() => Log(Description["help"]);
        public virtual void Pray() => Log(Description["prayer"]);
        public virtual void Stand() => Log(Description["stand"]);
        public virtual void Kill() => KillEvent(this, new StoryArgs());
        public virtual void Hurt(decimal damage) => Health -= damage;
        public virtual void Sit(IThing o) => Log(Description["sit"]);
        public virtual void Use(IUsable o) => o.Use();
        public virtual void Find(IThing o) => o.Find();
        public virtual void View(IThing o) => o.View();
        public virtual void Push(IPushable o) => o.Push();
        public virtual void Pull(IPushable o) => o.Pull();
        public virtual void Open(IOpenable o) => o.Open();
        public virtual void Shut(IOpenable o) => o.Shut();
        public virtual void Read(IReadable o) => o.Read();
        public virtual void Wear(IWearable o) => o.Wear();
        public virtual void Stow(IWearable o) => o.Stow();

        async Task OnGoto(IThing o, StoryArgs e) {
            Log($"{o} goes to the {e.Goal}.");
            WalkTarget.position = e.Goal.Position;
            await 1; }

        async Task OnKill(IThing o, StoryArgs e) {
            IsDead = true;
            Log(Description["death"]);
            await 1; }

        public virtual void Take(Thing o) {
            if (o==this) throw new StoryError(this["cannot take self"]);
            if (!(o is Item item)) throw new StoryError(this["cannot take thing"]);
            if (Items.Contains(item)) throw new StoryError(this["already take thing"]);
            item.Location = transform; Items.Add(item); item.Take();
        }

        public virtual void Drop(Thing thing) {
            if (!(thing is Item item)) throw new StoryError(Description["cannot drop"]);
            if (!Items.Contains(item)) throw new StoryError(Description["already drop"]);
            item.Drop(); Items.Remove(item);
            item.transform.parent = null;
            item.transform.position = transform.position+Vector3.forward;
        }

        public void Lock(Thing thing) {
            var list = from item in Items where item is Key select item as Key;
            if (thing is ILockable door && !door.IsLocked) list.ForEach(o => door.Lock(o));
            else throw new StoryError(Description["cannot lock"]);
        }

        public void Unlock(Thing thing) {
            var list = from item in Items where item is Key select item as Key;
            if (thing is ILockable door) list.First(o => o==door.LockKey); }

        protected void Do<T>(Thing o, StoryArgs e, Action<Actor,IThing> f) where T : IThing => Do<T>(o,e,o.Find<T>(),f);
        protected void Do<T>(Thing o, StoryArgs e, IEnumerable<T> a, Action<Actor,IThing> f) where T : IThing {
            var query =
                from item in Enumerable.Union(a.Cast<IThing>(), Items.Cast<IThing>())
                where item.Fits(e.Input) && item is T select item as Thing;
            if (!query.Any()) throw new StoryError(Description?["cannot nearby thing"]);
            if (query.Count()>1) throw new AmbiguityError(Description?["many nearby thing"], query.Cast<IThing>());
            e.Goal = query.First();
            if (o is Actor actor) f(actor, e.Goal as Thing);
            else throw new StoryError($"You can't do that to a {o}.");
        }

        public virtual void Do(Thing o, StoryArgs e) => o.Do();
        public virtual void Help(Thing o, StoryArgs e) => Help();
        public virtual void Pray(Thing o, StoryArgs e) => Pray();
        public virtual void Kill(Thing sender, StoryArgs args) { throw new MoralityError(
            sender.Description["attempt kill"], (o,e) => (o as Actor).Kill()); }

        public virtual void View(Thing o, StoryArgs e) => Do<Thing>(o,e, (t,a) => t.View(a as Thing));
        public virtual void Find(Thing o, StoryArgs e) => Do<Thing>(o,e, (t,a) => t.Find(a as Thing));
        public virtual void Goto(Thing o, StoryArgs e) => Do<Thing>(o,e, (t,a) => t.Goto(a as Thing));
        public virtual void Use(Thing o, StoryArgs e) => Do<IUsable>(o,e, (t,a) => t.Use(a as IUsable));
        public virtual void Sit(Thing o, StoryArgs e) => Do<Thing>(o,e, (t,a) => t.Sit(a as Thing));
        public virtual void Take(Thing o, StoryArgs e) => Do<Item>(o,e, (t,a) => t.Take(a as Item));
        public virtual void Drop(Thing o, StoryArgs e) => Do<Item>(o,e, (t,a) => t.Drop(a as Item));
        public virtual void Read(Thing o, StoryArgs e) => Do<IReadable>(o,e, (t,a) => t.Read(a as IReadable));
        public virtual void Push(Thing o, StoryArgs e) => Do<IPushable>(o,e, (t,a) => t.Push(a as IPushable));
        public virtual void Pull(Thing o, StoryArgs e) => Do<IPushable>(o,e, (t,a) => t.Pull(a as IPushable));
        public virtual void Open(Thing o, StoryArgs e) => Do<IOpenable>(o,e, (t,a) => t.Open(a as IOpenable));
        public virtual void Shut(Thing o, StoryArgs e) => Do<IOpenable>(o,e, (t,a) => t.Shut(a as IOpenable));
        public virtual void Wear(Thing o, StoryArgs e) => Do<IWearable>(o,e, (t,a) => t.Wear(a as IWearable));
        public virtual void Stow(Thing o, StoryArgs e) => Do<IWearable>(o,e, (t,a) => t.Stow(a as IWearable));
        public virtual void Lock(Thing o, StoryArgs e) => Do<ILockable>(o,e, (t,a) => t.Lock(a as Thing));
        public virtual void Unlock(Thing o, StoryArgs e) => Do<ILockable>(o,e, (t,a) => t.Unlock(a as Thing));

        protected override IEnumerable<Thing> Find<T>(float range, Vector3 position, LayerMask mask) =>
            from thing in Enumerable.Union(base.Find<T>(range,position,mask), Items.Cast<Thing>())
            where thing is T select thing as Thing;

        protected override void Awake() { base.Awake();
            rigidbody = GetComponent<Rigidbody>();
            WalkTarget = new GameObject($"{name} : walk to").transform;
            LookTarget = new GameObject($"{name} : look at").transform;
            WalkTarget.position = transform.position;
            LookTarget.position = transform.position;
            LookTarget.position += transform.forward + Vector3.up;
            onKill.AddListener((o,e) => StartAsync(() => OnKill(o,e)));
            onGoto.AddListener((o,e) => StartAsync(() => OnGoto(o,e)));
            KillEvent += (o,e) => onKill?.Call(o,e);
            GotoEvent += (o,e) => onGoto?.Call(o,e);
        }

        class Holdall<T> : IList<T> where T : Item {
            List<T> list = new List<T>();
            public bool IsReadOnly => false;
            public int Count => list.Count;
            public int Limit => 4;
            public T this[int i] { get {return list[i];} set {list[i]=value;} }
            public void Add(T o) => list.Add(o);
            public void Clear() => list.Clear();
            public void CopyTo(T[] a, int n) => list.CopyTo(a,n);
            public void Insert(int n, T o) => list.Insert(n, o);
            public void RemoveAt(int n) => list.RemoveAt(n);
            public bool Contains(T o) => list.Contains(o);
            public bool Remove(T o) => list.Remove(o);
            public int IndexOf(T o) => list.IndexOf(o);
            public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator() as IEnumerator;
        }

        new public class Data : Thing.Data {
            public bool dead {get;set;}
            public Map<Item.Data> items {get;set;}
            public override Object Deserialize(Object o) {
                var instance = base.Deserialize(o) as Actor;
                instance.IsDead = this.dead;
                var map = new Map<Item>();
                var items = instance.GetComponentsInChildren<Item>();
                foreach (var item in items) map[item.name] = item;
                instance.Items = map.Values.ToList();
                return instance;
            }
        }
    }
}
