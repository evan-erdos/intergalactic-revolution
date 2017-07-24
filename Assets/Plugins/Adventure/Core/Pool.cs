/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-11-30 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Pool : T[]
/// manages creation of a group of objects, memory efficient + injection
public class Pool<T> : IEnumerable<T> where T : Component {
    Func<T> func;
    Stack<T> stack = new Stack<T>();
    Queue<T> queue = new Queue<T>();
    public int Count => stack.Count + queue.Count;
    public Pool() { }
    public Pool(IEnumerable<T> list) { Add(list.ToArray()); }
    public Pool(GameObject original, int count=1) : this(count, () =>
        GameObject.Instantiate(original).GetComponent<T>()) { }
    public Pool(Func<T> func) : this(1,func) { }
    public Pool(int count, Func<T> func) { for (var i=1;i<count;++i) Drop(func()); }
    public IEnumerator<T> GetEnumerator() => queue.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => queue.GetEnumerator() as IEnumerator;
    void Drop(T o) { o.gameObject.SetActive(false); stack.Push(o); }
    public void Add(params T[] a) { foreach (var o in a) stack.Push(o); }
    public U Create<U>(Vector3 o) => Create<U>(o,Quaternion.identity);
    public U Create<U>(Transform o) => Create<U>(o.position,o.rotation);
    public U Create<U>(Vector3 p, Quaternion r) => Create(p,r).GetComponent<U>();
    public T Create(Vector3 o) => Create(o, Quaternion.identity);
    public T Create(Transform o) => Create(o.position, o.rotation);
    public T Create(Vector3 position, Quaternion rotation) {
        if (!stack.Any() && queue.Any()) Drop(queue.Dequeue());
        var instance = stack.Pop();
        instance.transform.parent = null;
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.GetComponent<IResettable>()?.Reset();
        instance.gameObject.SetActive(true);
        queue.Enqueue(instance);
        return instance;
    }
}
