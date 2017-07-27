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
        [SerializeField] Event<StoryArgs> onKill = new Event<StoryArgs>();
        [SerializeField] Event<StoryArgs> onGoto = new Event<StoryArgs>();
        public event AdventureAction<StoryArgs> KillEvent {add{onKill.Add(value);} remove{onKill.Remove(value);}}
        public event AdventureAction<StoryArgs> GotoEvent {add{onGoto.Add(value);} remove{onGoto.Remove(value);}}
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

        async Task OnGoto(StoryArgs e) {
            Log($"{e.Sender} goes to the {e.Goal}.");
            WalkTarget.position = e.Goal.Position; await 1; }

        async Task OnKill(StoryArgs e) { IsDead = true; Log(Description["death"]); await 1; }

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


        public virtual void Goto(IThing o) => onGoto?.Call(new StoryArgs {
            Sender = this, Message = $"go to {o}", Goal = o });

        public override void Do() => Talk();
        public virtual void Take() => Find<Item>().ForEach(o => Take(o));
        public virtual void Drop() => Items.ForEach(item => Drop(item as Thing));
        public virtual void Talk() => Log(Description["talk"]);
        public virtual void Help() => Log(Description["help"]);
        public virtual void Pray() => Log(Description["prayer"]);
        public virtual void Stand() => Log(Description["stand"]);
        public virtual void Hurt(decimal o) => Health -= o;
        public virtual void Kill(Actor o) => Kill(new StoryArgs { Sender = this, Message = $"kill {o}", Goal = o });
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
        public override void View(StoryArgs e=null) => Do<Thing>(e,(t,a) => t.View(a as Thing));
        public override void Find(StoryArgs e=null) => Do<Thing>(e,(t,a) => t.Find(a as Thing));
        public virtual void Goto(StoryArgs e=null) => Do<Thing>(e,(t,a) => t.Goto(a as Thing));
        public virtual void Use(StoryArgs e=null) => Do<IUsable>(e,(t,a) => t.Use(a as IUsable));
        public virtual void Sit(StoryArgs e=null) => Do<Thing>(e,(t,a) => t.Sit(a as Thing));
        public virtual void Take(StoryArgs e=null) => Do<Item>(e,(t,a) => t.Take(a as Item));
        public virtual void Drop(StoryArgs e=null) => Do<Item>(e,(t,a) => t.Drop(a as Item));
        public virtual void Read(StoryArgs e=null) => Do<IReadable>(e,(t,a) => t.Read(a as IReadable));
        public virtual void Push(StoryArgs e=null) => Do<IPushable>(e,(t,a) => t.Push(a as IPushable));
        public virtual void Pull(StoryArgs e=null) => Do<IPushable>(e,(t,a) => t.Pull(a as IPushable));
        public virtual void Open(StoryArgs e=null) => Do<IOpenable>(e,(t,a) => t.Open(a as IOpenable));
        public virtual void Shut(StoryArgs e=null) => Do<IOpenable>(e,(t,a) => t.Shut(a as IOpenable));
        public virtual void Wear(StoryArgs e=null) => Do<IWearable>(e,(t,a) => t.Wear(a as IWearable));
        public virtual void Stow(StoryArgs e=null) => Do<IWearable>(e,(t,a) => t.Stow(a as IWearable));
        public virtual void Lock(StoryArgs e=null) => Do<ILockable>(e,(t,a) => t.Lock(a as Thing));
        public virtual void Unlock(StoryArgs e=null) => Do<ILockable>(e,(t,a) => t.Unlock(a as Thing));
        public virtual void Help(StoryArgs e=null) => Help();
        public virtual void Pray(StoryArgs e=null) => Pray();
        public virtual void Kill(StoryArgs e=null) {
            throw new MoralityError((e.Sender as Thing)["attempt kill"], o =>
                (o.Sender as Actor).onKill?.Call(e ?? new StoryArgs { Sender = this })); }
        public virtual void Do(StoryArgs e=null) { if (e.Sender is IThing thing) thing.Do(); }
        protected void Do<T>(StoryArgs e, Action<Actor,IThing> f) where T : IThing => Do<T>(e,e.Sender.Find<T>(),f);
        protected void Do<T>(StoryArgs e, IEnumerable<T> a, Action<Actor,IThing> f) where T : IThing {
            var query =
                from item in Enumerable.Union(a.Cast<IThing>(), Items.Cast<IThing>())
                where item.Fits(e.Input) && item is T select item as Thing;
            if (!query.Any()) throw new StoryError(Description?["cannot nearby thing"]);
            if (query.Count()>1) throw new AmbiguityError(Description?["many nearby thing"], query.Cast<IThing>());
            e.Goal = query.First();
            if (e.Sender is Actor actor) f(actor, e.Goal as Thing);
            else throw new StoryError($"You can't do that to a {e.Sender}.");
        }

        protected override IEnumerable<Thing> Find<T>(float range, Vector3 position) =>
            from thing in Enumerable.Union(base.Find<T>(range, position), Items.Cast<Thing>())
            where thing is T select thing as Thing;

        protected override void Awake() { base.Awake();
            rigidbody = Get<Rigidbody>();
            KillEvent += e => StartAsync(() => OnKill(e));
            GotoEvent += e => StartAsync(() => OnGoto(e));
            WalkTarget = new GameObject($"{name} : walk to").transform;
            LookTarget = new GameObject($"{name} : look at").transform;
            WalkTarget.position = transform.position;
            LookTarget.position = transform.position;
            LookTarget.position += transform.forward + Vector3.up;
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
                foreach (var i in instance.GetChildren<Item>()) map[i.name] = i;
                instance.Items = map.Values.ToList();
                return instance;
            }
        }
    }
}
