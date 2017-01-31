/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-22 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Adventure {
    public abstract class NetObject : NetworkBehaviour, IObject {
        Regex regex = new Regex("\b(object)\b");
        Dictionary<string,Func<IEnumerator>> coroutines =
            new Dictionary<string,Func<IEnumerator>>();
        public bool AreAnyYielding => coroutines.Count>0;
        public virtual float Range => 5f;
        public virtual string Name => name;
        public virtual Vector3 Position => transform.position;
        public virtual LayerMask Mask {get;protected set;}
        public virtual void Deactivate() => ClearCoroutines();
        public virtual void Create() => ClearCoroutines();
        public virtual bool Fits(string pattern) => regex.IsMatch(pattern);
        public override string ToString() => "${name}";
        public static bool operator !(NetObject o) => o==null;
        // responsibilities for enabling and disabling
        // protected virtual void OnDisable() => Disable();
        // protected virtual void OnEnable() => Create();

        public void If(bool condition, Action then) { if (condition) then(); }
        public void If(Func<bool> cond, Action then) { if (cond()) then(); }

        public Transform GetOrAdd(string name) {
            var instance = transform.Find(name);
            if (!instance) {
                instance = new GameObject(name).transform;
                instance.parent = transform; } return instance; }

        public T GetOrAdd<T>() where T : Component => GetOrAdd<T,T>();
        public T GetOrAdd<T,U>() where T : Component where U : T {
            var component = Get<T>();
            if (component is null) component = gameObject.AddComponent<U>();
            return component;
        }

        public T Get<T>() => GetComponentOrNull<T>(GetComponent<T>());
        public T GetParent<T>() => GetComponentOrNull<T>(GetComponentInParent<T>());
        public T GetChild<T>() => GetComponentOrNull<T>(GetComponentInChildren<T>());
        T GetComponentOrNull<T>(T o) => (o==null)?default(T):o;

        public List<T> Find<T>() where T : IThing => Find<T>(Range);
        public List<T> Find<T>(float r) where T : IThing => Find<T>(r,transform);
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
        public T Create<T>(
                        GameObject original,
                        Vector3 position,
                        Quaternion rotation) =>
            GetComponentOrNull<T>(
                Create(original,position,rotation).GetComponent<T>());

        public GameObject Create(GameObject original) =>
            Create(original,transform.position, transform.rotation);
        public GameObject Create(GameObject original, Vector3 position) =>
            Create(original, transform.position, Quaternion.identity);

        // public GameObject Create(
        //                 GameObject original,
        //                 Vector3 position,
        //                 Quaternion rotation) =>
        //         Instantiate(original,position,rotation) as GameObject;

        public GameObject Create(
                        GameObject original,
                        Vector3 position,
                        Quaternion rotation) {
            CmdCreate(original,position,rotation);
            return lastInstanceMade; }

        GameObject lastInstanceMade;

        [Command] public void CmdCreate(
                        GameObject original,
                        Vector3 position,
                        Quaternion rotation) {
            var instance = Instantiate(original,position,rotation) as GameObject;
            instance.GetComponent<IObject>().Create();
            instance.GetComponentInChildren<IObject>().Create();
            NetworkServer.Spawn(instance);
            lastInstanceMade = instance;
        }

        protected void Wait(float wait, Action func) {
            StartCoroutine(WaitingSeconds());
            IEnumerator WaitingSeconds() {
                yield return new WaitForSeconds(wait); func(); } }

        protected Coroutine Loop(YieldInstruction wait, Action func) {
            return StartCoroutine(Looping());
            IEnumerator Looping() { while (true) yield return Wait(wait,func); } }

        protected Coroutine Wait(YieldInstruction wait, Action func) {
            return StartCoroutine(Waiting());
            IEnumerator Waiting() { yield return wait; func(); } }

        public void ClearCoroutines() => coroutines.Clear();
        public bool IsYielding(string name) => coroutines.ContainsKey(name);
        public void StartSemaphore(Func<IEnumerator> coroutine) =>
            StartSemaphore(coroutine.Method.Name, coroutine);
        public void StartSemaphore(string name, Func<IEnumerator> coroutine) {
            if (!coroutines.ContainsKey(name))
                StartCoroutine(Waiting(name,coroutine)); }
        IEnumerator Waiting(string name, Func<IEnumerator> coroutine) {
            coroutines[name] = coroutine;
            yield return StartCoroutine(coroutine());
            coroutines.Remove(name);
        }

        public class Data {
            public string name {get;set;}
            public virtual BaseObject Deserialize(BaseObject o) => o;
            public virtual void Merge(BaseObject.Data data) => name = data.name;
        }
    }
}



