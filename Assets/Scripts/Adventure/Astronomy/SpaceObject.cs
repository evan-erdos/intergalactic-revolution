/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-22 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure.Astronomy {
    public abstract class SpaceObject : MonoBehaviour, ISpaceObject {
        Semaphore semaphore;
        public virtual Vector3 Position => transform.position;
        protected virtual void Awake() =>
            semaphore = new Semaphore(StartCoroutine);

        public Transform FindOrAdd(string name) {
            var go = transform.Find(name);
            if (!go) {
                go = new GameObject(name).transform;
                go.parent = transform;
            } return go;
        }

        public T GetOrAdd<T>() where T : Component => GetOrAdd<T,T>();
        public T GetOrAdd<T,U>() where T : Component where U : T {
            var component = GetComponent<T>();
            if (!component) component = gameObject.AddComponent<U>();
            return component;
        }

        public override string ToString() => name;
        public static bool operator !(SpaceObject o) => o==null;
        protected void StartSemaphore(Func<IEnumerator> c) =>
            semaphore.Invoke(c);
        protected void StartSemaphore<T>(Func<IEnumerator> c, T args) =>
            semaphore.Invoke(c); //(args)
    }
}
