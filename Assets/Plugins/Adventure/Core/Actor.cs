/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-31 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Inventories;
using Adventure.Locales;

namespace Adventure {
    public class Actor : Thing, IActor {
        new protected Rigidbody rigidbody;
        [SerializeField] protected StoryEvent onKill;
        [SerializeField] protected StoryEvent onGoto;
        public event StoryAction KillEvent;
        public event StoryAction GotoEvent;
        public virtual bool IsDead {get;set;}
        public override float Range => 16;
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
            var room = value.GetComponentInParent<Room>();
            if (room) base.Location = room.Location;
            else throw new StoryError(Description["cannot goto"]); } }

        public virtual void Kill(Actor thing) => KillEvent(this, new StoryArgs {
            Message = $"kill {thing}", Goal = thing });

        public virtual void Goto(IThing thing) => GotoEvent(this, new StoryArgs {
            Message = $"go to {thing}", Goal = thing });

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

        void OnGoto(IThing thing) {
            StartSemaphore(GoingTo);
            IEnumerator GoingTo() {
                Log($"The Monk goes to the {thing}.");
                WalkTarget.position = thing.Position;
                yield return new WaitForSeconds(1f);
            }
        }

        void OnKill() {
            StartSemaphore(Killing);
            IEnumerator Killing() {
                IsDead = true;
                Log(Description["death"]);
                yield return new WaitForSeconds(1f);
            }
        }

        public virtual void Take(Thing thing) {
            if (thing==this)
                throw new StoryError(Description["cannot take self"]);
            if (!(thing is Item item))
                throw new StoryError(Description["cannot take thing"]);
            if (Items.Contains(item))
                throw new StoryError(Description["already take thing"]);
            item.Location = transform;
            Items.Add(item);
            item.Take();
        }

        public virtual void Drop(Thing thing) {
            if (!(thing is Item item))
                throw new StoryError(Description["cannot drop"]);
            if (!Items.Contains(item))
                throw new StoryError(Description["already drop"]);
            item.Drop();
            Items.Remove(item);
            item.transform.parent = null;
            item.transform.position = transform.position+Vector3.forward;
        }

        public void Lock(Thing thing) {
            var list =
                from item in Items
                where item is Key
                select item as Key;
            if (thing is ILockable door && !door.IsLocked)
                list.ToList().ForEach(item => door.Lock(item));
            else throw new StoryError(Description["cannot lock"]);
        }

        public void Unlock(Thing thing) {
            var list =
                from item in Items
                where item is Key
                select item as Key;
            if (thing is ILockable door)
                list.First(key => key==door.LockKey);
        }

        protected void Do<T>(
                        Thing thing,
                        StoryArgs args,
                        IEnumerable<T> list,
                        Action<Actor,Thing> then) where T : IThing {
            var query =
                from item in Enumerable.Union(
                    list.Cast<IThing>(), Items.Cast<IThing>())
                where item.Fits(args.Input) && item is T
                select item as Thing;
            if (!query.Any())
                throw new StoryError(Description?["cannot nearby thing"]);
            if (query.Count()>1)
                throw new AmbiguityError(
                    Description?["many nearby thing"], query.Cast<IThing>());
            args.Goal = query.First();
            if (thing is Actor actor) then(actor, args.Goal as Thing);
            else throw new StoryError($"You can't do that to a {thing}.");
        }

        public virtual void Do(Thing o, StoryArgs e) => o.Do();
        public virtual void Help(Thing o, StoryArgs e) => Help();
        public virtual void Pray(Thing o, StoryArgs e) => Pray();
        public virtual void Kill(Thing sender, StoryArgs args) {
            throw new MoralityError(
                message: sender.Description["attempt kill"],
                then: (o,e) => (o as Actor).Kill()); }

        public virtual void View(Thing sender, StoryArgs args) => Do<Thing>(
            sender, args, sender.Find<Thing>(), (o,e) => o.View(e as Thing));

        public virtual void Find(Thing sender, StoryArgs args) => Do<Thing>(
            sender, args, sender.Find<Thing>(), (o,e) => o.Find(e as Thing));

        public virtual void Goto(Thing sender, StoryArgs args) => Do<Thing>(
            sender, args, sender.Find<Thing>(), (o,e) => o.Goto(e as Thing));

        public virtual void Use(Thing sender, StoryArgs args) => Do<IUsable>(
            sender, args, sender.Find<IUsable>(), (o,e) => o.Use(e as IUsable));

        public virtual void Sit(Thing sender, StoryArgs args) => Do<Thing>(
            sender, args, sender.Find<Thing>(), (o,e) => o.Sit(e as Thing));

        public virtual void Take(Thing sender, StoryArgs args) => Do<Item>(
            sender, args, sender.Find<Item>(), (o,e) => o.Take(e as Item));

        public virtual void Drop(Thing sender, StoryArgs args) => Do<Item>(
            sender, args, sender.Find<Item>(0), (o,e) => o.Drop(e as Item));

        public virtual void Read(Thing sender, StoryArgs args) => Do<IReadable>(
            sender, args, sender.Find<IReadable>(), (o,e) => o.Read(e as IReadable));

        public virtual void Push(Thing sender, StoryArgs args) => Do<IPushable>(
            sender, args, sender.Find<IPushable>(), (o,e) => o.Push(e as IPushable));

        public virtual void Pull(Thing sender, StoryArgs args) => Do<IPushable>(
            sender, args, sender.Find<IPushable>(), (o,e) => o.Pull(e as IPushable));

        public virtual void Open(Thing sender, StoryArgs args) => Do<IOpenable>(
            sender, args, sender.Find<IOpenable>(), (o,e) => o.Open(e as IOpenable));

        public virtual void Shut(Thing sender, StoryArgs args) => Do<IOpenable>(
            sender, args, sender.Find<IOpenable>(), (o,e) => o.Shut(e as IOpenable));

        public virtual void Wear(Thing sender, StoryArgs args) => Do<IWearable>(
            sender, args, sender.Find<IWearable>(0), (o,e) => o.Wear(e as IWearable));

        public virtual void Stow(Thing sender, StoryArgs args) => Do<IWearable>(
            sender, args, sender.Find<IWearable>(0), (o,e) => o.Stow(e as IWearable));

        public virtual void Lock(Thing sender, StoryArgs args) => Do<ILockable>(
            sender, args, sender.Find<ILockable>(), (o,e) => o.Lock(e as Thing));

        public virtual void Unlock(Thing sender, StoryArgs args) => Do<ILockable>(
            sender, args, sender.Find<ILockable>(), (o,e) => o.Unlock(e as Thing));

        protected override IEnumerable<Thing> Find<T>(
                        float range,
                        Vector3 position,
                        LayerMask mask) =>
            from thing in Enumerable.Union(
                base.Find<T>(range, position, mask),
                Items.Cast<Thing>())
            where thing is T
            select thing as Thing;

        protected override void Awake() { base.Awake();
            rigidbody = GetComponent<Rigidbody>();
            WalkTarget = new GameObject($"{name}:walk-target").transform;
            LookTarget = new GameObject($"{name}:look-target").transform;
            WalkTarget.position = transform.position;
            LookTarget.position = transform.position;
            LookTarget.position += transform.forward + Vector3.up;
            onKill.AddListener((o,e) => OnKill());
            onGoto.AddListener((o,e) => OnGoto(e.Goal));
            KillEvent += (o,e) => onKill?.Invoke(o,e);
            GotoEvent += (o,e) => onGoto?.Invoke(o,e);
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
            IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) GetEnumerator();
        }

        new public class Data : Thing.Data {
            public bool dead {get;set;}
            public Map<Item.Data> items {get;set;}
            public override BaseObject Deserialize(BaseObject o) {
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
