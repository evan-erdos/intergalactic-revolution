/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-22 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Adventure {
    public abstract class NetObject : NetworkBehaviour, IObject {
        Regex regex = new Regex("\b(object)\b");
        HashSet<string> threads = new HashSet<string>();
        [SerializeField] protected RealityEvent onCreate = new RealityEvent();
        public bool AreAnyYielding => threads.Count>0;
        public virtual float Radius => 5;
        public virtual string Name => name;
        public virtual Vector3 Position => transform.position;
        public virtual LayerMask Mask {get;protected set;}
        public virtual bool Fits(string pattern) => regex.IsMatch(pattern);
        public override string ToString() => "${name}";
        public static bool operator !(NetObject o) => o==null;
        public event RealityAction CreateEvent;


        public virtual void Init() {
            onCreate.AddListener((o,e) => ClearSemaphore());
            CreateEvent += (o,e) => onCreate?.Invoke(o,e);
        }

        public Transform GetOrAdd(string name) {
            var o = transform.Find(name); if (o) return o;
            o = new GameObject(name).transform; o.parent = transform; return o; }

        public T GetOrAdd<T>() where T : Component => GetOrAdd<T,T>();
        public T GetOrAdd<T,U>() where T : Component where U : T {
            var o = GetComponent<T>(); if (o) return o;
            o = gameObject.AddComponent<U>(); return o; }

        public List<T> Find<T>() where T : IThing => Find<T>(Radius);
        public List<T> Find<T>(float radius) where T : IThing => Find<T>(radius,transform);
        public List<T> Find<T>(float radius, Transform location) where T : IThing =>
            Find<T>(radius, location.position, Mask).Cast<T>().ToList();

        protected virtual IEnumerable<Thing> Find<T>(
                        float range, Vector3 position, LayerMask mask) where T : IThing =>
            from collider in Physics.OverlapSphere(position, range, mask)
            let thing = collider.GetComponentInParent<T>()
            where thing!=null select thing as Thing;


        GameObject lastInstanceMade;
        public GameObject Create(GameObject original, Vector3 position, Quaternion rotation) {
            CmdCreate(original,position,rotation); return lastInstanceMade; }

        [Command] public void CmdCreate(GameObject original, Vector3 position, Quaternion rotation) {
            var o = Instantiate(original,position,rotation);
            if (o.Get<ICreatable>() is ICreatable c) { c.Init(); c.Create(); }
            NetworkServer.Spawn(o); lastInstanceMade = o; }

        public void Create() => Create(this, new RealityArgs());
        public void Create(IObject o, RealityArgs e) => CreateEvent(o,e);
        public GameObject Create(GameObject original) => Create(original, transform.position, transform.rotation);
        public GameObject Create(GameObject original, Vector3 position) => Create(original, transform.position, Quaternion.identity);
        // public GameObject Create(GameObject original, Vector3 position, Quaternion rotation) =>
        //     Instantiate<GameObject>(original, position, rotation);
        public T Create<T>(GameObject original) => Create<T>(original, transform.position, transform.rotation);
        public T Create<T>(GameObject original, Vector3 position) => Create<T>(original, position, Quaternion.identity);
        public T Create<T>(GameObject original, Vector3 position, Quaternion rotation) => Create(original,position,rotation).Get<T>();
        public bool If(Func<bool> cond, Action then) => If (cond(), then);
        public bool If(bool cond, Action then) { if (cond) then(); return cond; }
        public bool If<T>(T cond, Action<T> then) { var b = cond!=null; if (b) then(cond); return b; }
        public T Get<T>() => gameObject.Get<T>();
        public T GetParent<T>() => gameObject.GetParent<T>();
        public T GetChild<T>() => gameObject.GetChild<T>();
        public List<T> GetChildren<T>() => gameObject.GetChildren<T>();
        public void ClearSemaphore() => threads.Clear();
        public bool IsYielding(string s) => threads.Contains(s);
        protected Coroutine Wait(Action f) => StartCoroutine(WaitDelay(0,f));
        protected Coroutine Wait(float w, Action f) => StartCoroutine(WaitDelay(w,f));
        protected Coroutine Loop(YieldInstruction w, Action f) => StartCoroutine(Looping(w,f));
        protected Coroutine Wait(YieldInstruction w, Action f) => StartCoroutine(WaitingFor(w,f));
        public void StartSemaphore(Func<IEnumerator> f) => StartSemaphore(f, f.Method.Name);
        public void StartAsync(Func<Task> f) => StartAsync(f, f.Method.Name);
        async void StartAsync(Func<Task> f,string s) { if (!threads.Contains(s)) { threads.Add(s); await f(); threads.Remove(s); } }
        void StartSemaphore(Func<IEnumerator> f, string s) { if (!threads.Contains(s)) StartCoroutine(Waiting(s,f)); }
        IEnumerator WaitDelay(float w, Action f) { yield return new WaitForSeconds(w); f(); }
        IEnumerator Looping(YieldInstruction w, Action f) { while (true) yield return Wait(w,f); }
        IEnumerator WaitingFor(YieldInstruction w, Action f) { yield return w; f(); }
        IEnumerator Waiting(string s, Func<IEnumerator> f) { threads.Add(s); yield return StartCoroutine(f()); threads.Remove(s); }


        public class Data {
            public string name {get;set;}
            public virtual Adventure.Object Deserialize(Adventure.Object o) => o;
            public virtual void Merge(Adventure.Object.Data o) => name = o.name;
        }
    }
}
