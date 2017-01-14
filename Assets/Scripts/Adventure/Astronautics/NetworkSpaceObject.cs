/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-22 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Adventure.Astronautics {
    public abstract class NetworkSpaceObject : NetworkBehaviour, ISpaceObject {
        Semaphore semaphore;
        public string Name => name;
        public (float x,float y,float z) Position => transform.position.tuple();
        protected void StartSemaphore(Func<IEnumerator> c) => semaphore.Invoke(c);
        public virtual bool Fits(string s) => new Regex("\b(object)\b").IsMatch(s);
        public override string ToString() => $"{name} - ({1} ton)";
        // public virtual void OnDeactivate() => semaphore?.Clear();
        protected virtual void OnDisable() => semaphore?.Clear();
        protected virtual void OnEnable() => Create();
        public virtual void Create() => semaphore = new Semaphore(StartCoroutine);

        public Transform GetOrAdd(string name) {
            var instance = transform.Find(name);
            if (!instance) {
                instance = new GameObject(name).transform;
                instance.parent = transform; } return instance; }

        public T GetOrAdd<T>() where T : Component => GetOrAdd<T,T>();
        public T GetOrAdd<T,U>() where T : Component where U : T {
            var component = GetComponent<T>();
            if (!component) component = gameObject.AddComponent<U>();
            return component; }

        public T Get<T>() => GetComponentOrNull<T>(GetComponent<T>());
        public T GetParent<T>() => GetComponentOrNull<T>(GetComponentInParent<T>());
        public T GetChild<T>() => GetComponentOrNull<T>(GetComponentInChildren<T>());
        T GetComponentOrNull<T>(T component) => (component==null)?default(T):component;

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
                        Quaternion rotation) {
            var instance = Instantiate(original,position,rotation) as GameObject;
            instance.GetComponent<ISpaceObject>().Create();
            instance.GetComponentInChildren<ISpaceObject>().Create();
            CmdCreate(instance); return instance; }

        [Command] public void CmdCreate(GameObject o) => NetworkServer.Spawn(o);

        protected Coroutine Loop(YieldInstruction wait, Action func) {
            return StartCoroutine(Looping());
            IEnumerator Looping() { while (true) yield return Wait(wait,func); } }

        protected Coroutine Wait(YieldInstruction wait, Action func) {
            return StartCoroutine(Waiting());
            IEnumerator Waiting() { yield return wait; func(); } }
    }
}
