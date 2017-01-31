/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-22 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure {
    public abstract class BaseObject : MonoBehaviour, IObject {
        protected Regex regex = new Regex("\b(object)\b");
        protected Map<Routine> coroutines = new Map<Routine>();
        public static bool operator !(BaseObject o) => o==null;
        public bool AreAnyYielding => coroutines.Count>0;
        public virtual string Name => name;
        public virtual Vector3 Position => transform.position;
        public virtual LayerMask Mask {get;protected set;}
        public virtual void Deactivate() => ClearCoroutines();
        public virtual void Create() => ClearCoroutines();
        public virtual bool Fits(string pattern) => regex.IsMatch(pattern);
        public override string ToString() => "${name}";
        public void If(bool cond, Action then) { if (cond) then(); }
        public void If(Func<bool> cond, Action then) { if (cond()) then(); }
        public T Get<T>() => GetComponentOrNull<T>(GetComponent<T>());
        public T GetParent<T>() => GetComponentOrNull<T>(GetComponentInParent<T>());
        public T GetChild<T>() => GetComponentOrNull<T>(GetComponentInChildren<T>());
        T GetComponentOrNull<T>(T o) => (o==null)?default(T):o;

        public T GetOrAdd<T>() where T : Component => GetOrAdd<T,T>();
        public T GetOrAdd<T,U>() where T : Component where U : T {
            var component = Get<T>();
            if (!component) component = gameObject.AddComponent<U>();
            return component;
        }

        public Transform GetOrAdd(string name) {
            var instance = transform.Find(name);
            if (!instance) {
                instance = new GameObject(name).transform;
                instance.parent = transform;
            } return instance;
        }

        public List<T> Find<T>(float r=5) where T : IThing => Find<T>(r,transform);
        public List<T> Find<T>(float r, Transform l) where T : IThing =>
            Find<T>(r, l.position, Mask).Cast<T>().ToList();
        protected virtual IEnumerable<Thing> Find<T>(
                        float range,
                        Vector3 position,
                        LayerMask mask) where T : IThing =>
            from collider in Physics.OverlapSphere(position, range, mask)
            let thing = collider.GetComponentInParent<T>()
            where thing!=null
            select thing as Thing;


        public T Create<T>(GameObject original) =>
            Create<T>(original, transform.position, transform.rotation);
        public T Create<T>(GameObject original,Vector3 position) =>
            Create<T>(original,position,Quaternion.identity);
        public T Create<T>(GameObject original,Vector3 position,Quaternion rotation) =>
            GetComponentOrNull<T>(Create(original,position,rotation).GetComponent<T>());

        public GameObject Create(GameObject original) =>
            Create(original,transform.position, transform.rotation);
        public GameObject Create(GameObject original, Vector3 position) =>
            Create(original, transform.position, Quaternion.identity);
        public GameObject Create(
                        GameObject original,
                        Vector3 position,
                        Quaternion rotation) =>
            Instantiate(original,position,rotation) as GameObject;

        protected void Wait(float wait, Action func) {
            StartCoroutine(WaitSecs());
            IEnumerator WaitSecs() { yield return new WaitForSeconds(wait); func(); } }

        protected Coroutine Loop(YieldInstruction wait, Action func) {
            return StartCoroutine(Looping());
            IEnumerator Looping() { while (true) yield return Wait(wait,func); } }

        protected Coroutine Wait(Action func) {
            return StartCoroutine(Waiting());
            IEnumerator Waiting() { yield return null; func(); } }

        protected Coroutine Wait(YieldInstruction wait, Action func) {
            return StartCoroutine(Waiting());
            IEnumerator Waiting() { yield return wait; func(); } }

        protected void ClearCoroutines() => coroutines.Clear();
        protected bool IsYielding(string o) => coroutines.ContainsKey(o);
        protected void StartSemaphore(Routine o) => StartSemaphore(o.Method.Name, o);
        protected void StartSemaphore(string name, Routine func) {
            if (!coroutines.ContainsKey(name)) StartCoroutine(Waiting(name,func)); }

        IEnumerator Waiting(string name, Routine func) {
            coroutines[name] = func;
            yield return StartCoroutine(func());
            coroutines.Remove(name);
        }

        public class Data {
            public string name {get;set;}
            public virtual BaseObject Deserialize(BaseObject o) => o;
            public virtual void Merge(BaseObject.Data data) => name = data.name;
        }
    }
}
