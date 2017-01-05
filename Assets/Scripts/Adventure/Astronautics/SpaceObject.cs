/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-22 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure.Astronautics {
    public abstract class SpaceObject : MonoBehaviour, ISpaceObject {
        Regex regex = new Regex("\b(object)\b");
        Semaphore semaphore;
        public virtual (float x,float y,float z) Position => transform.position.ToTuple();
        public virtual void Init() => semaphore = new Semaphore(StartCoroutine);

        public T Get<T>() {
            var component = GetComponent<T>();
            return (component==null)?default(T):component;
        }

        public T GetOrAdd<T>() where T : Component => GetOrAdd<T,T>();
        public T GetOrAdd<T,U>() where T : Component where U : T {
            var component = GetComponent<T>();
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

        public T Create<T>(GameObject original) =>
            Create<T>(original, transform.position, transform.rotation);
        public T Create<T>(GameObject original, Vector3 position) =>
            Create<T>(original,position,Quaternion.identity);
        public T Create<T>(GameObject original, Vector3 position, Quaternion rotation) {
            var instance = Instantiate(original,position,rotation) as GameObject;
            var component = instance.GetComponent<T>();
            return (component==null)?default(T):component;
        }


        protected Coroutine Wait(YieldInstruction wait, Action func) {
            return StartCoroutine(Waiting());
            IEnumerator Waiting() { yield return wait; func(); }
        }

        protected void StartSemaphore(Func<IEnumerator> c) => semaphore.Invoke(c);
        public virtual bool Fits(string pattern) => regex.IsMatch(pattern);
        public override string ToString() => $"{name} - ({1} ton)";

        public class Data {
            public string name {get;set;}
            public virtual SpaceObject Deserialize(SpaceObject o) => o;
            public virtual void Merge(SpaceObject.Data o) => name = o.name;
        }
    }
}
