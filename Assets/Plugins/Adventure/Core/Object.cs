/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-22 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure {
    public abstract class Object : MonoBehaviour, IObject {
        protected Regex regex = new Regex("\b(object)\b");
        Map<Func<IEnumerator>> coroutines = new Map<Func<IEnumerator>>();
        [SerializeField] protected RealityEvent onCreate = new RealityEvent();
        public bool AreAnyYielding => coroutines.Count>0;
        public virtual string Name => name;
        public virtual float Radius => 2;
        public virtual Vector3 Position => transform.position;
        public virtual LayerMask Mask {get;protected set;}
        public event RealityAction CreateEvent;

        public virtual void Init() {
            onCreate.AddListener((o,e) => ClearSemaphore());
            CreateEvent += (o,e) => onCreate?.Invoke(o,e);
        }

        public Transform GetOrAdd(string name) {
            var instance = transform.Find(name);
            if (instance) return instance;
            instance = new GameObject(name).transform;
            instance.parent = transform;
            return instance;
        }

        public T GetOrAdd<T>() where T : Component => GetOrAdd<T,T>();
        public T GetOrAdd<T,U>() where T : Component where U : T {
            var instance = GetComponent<T>();
            if (instance) return instance;
            instance = gameObject.AddComponent<U>();
            return instance;
        }

        public List<T> Find<T>() where T : IThing => Find<T>(Radius);
        public List<T> Find<T>(float radius) where T : IThing => Find<T>(radius,transform);
        public List<T> Find<T>(float radius, Transform location) where T : IThing =>
            Find<T>(radius, location.position, Mask).Cast<T>().ToList();

        protected virtual IEnumerable<Thing> Find<T>(
                        float range, Vector3 position, LayerMask mask) where T : IThing =>
            from collider in Physics.OverlapSphere(position, range, mask)
            let thing = collider.GetComponentInParent<T>()
            where thing!=null
            select thing as Thing;

        public virtual bool Fits(string pattern) => regex.IsMatch(pattern);
        public void Create() => Create(this, new RealityArgs());
        public void Create(IObject o, RealityArgs e) => CreateEvent(o,e);
        public GameObject Create(GameObject original) => Create(original, transform.position, transform.rotation);
        public GameObject Create(GameObject original, Vector3 position) =>
            Create(original, transform.position, Quaternion.identity);
        public GameObject Create(GameObject original, Vector3 position, Quaternion rotation) =>
            Instantiate<GameObject>(original, position, rotation);
        public T Create<T>(GameObject original) => Create<T>(original, transform.position, transform.rotation);
        public T Create<T>(GameObject original, Vector3 position) =>
            Create<T>(original, position, Quaternion.identity);
        public T Create<T>(GameObject original, Vector3 position, Quaternion rotation) =>
            Create(original,position,rotation).Get<T>();
        public bool If(Func<bool> cond, Action then) => If (cond(), then);
        public bool If(bool cond, Action then) { if (cond) then(); return cond; }
        public bool If<T>(T cond, Action<T> then) { var b = cond!=null; if (b) then(cond); return b; }
        public T Get<T>() => gameObject.Get<T>();
        public T GetParent<T>() => gameObject.GetParent<T>();
        public T GetChild<T>() => gameObject.GetChild<T>();
        public List<T> GetChildren<T>() => gameObject.GetChildren<T>();
        public void ClearSemaphore() => coroutines.Clear();
        public bool IsYielding(string name) => coroutines.ContainsKey(name);
        protected Coroutine Wait(Action func) => StartCoroutine(WaitDelay(0,func));
        protected Coroutine Wait(float wait, Action func) => StartCoroutine(WaitDelay(wait,func));
        protected Coroutine Loop(YieldInstruction wait, Action func) => StartCoroutine(Looping(wait,func));
        protected Coroutine Wait(YieldInstruction wait, Action func) => StartCoroutine(WaitingFor(wait,func));
        public void StartSemaphore(Func<IEnumerator> func) {
            if (!coroutines.ContainsKey(func.Method.Name)) StartCoroutine(Waiting(func.Method.Name,func)); }
        IEnumerator WaitDelay(float wait, Action func) { yield return new WaitForSeconds(wait); func(); }
        IEnumerator Looping(YieldInstruction wait, Action func) { while (true) yield return Wait(wait,func); }
        IEnumerator WaitingFor(YieldInstruction wait, Action func) { yield return wait; func(); }
        IEnumerator Waiting(string name, Func<IEnumerator> func) {
            coroutines[name] = func; yield return StartCoroutine(func()); coroutines.Remove(name); }

        public class Data {
            public string name {get;set;}
            public virtual Adventure.Object Deserialize(Adventure.Object o) => o;
            public virtual void Merge(Adventure.Object.Data data) => name = data.name;
        }
    }
}
