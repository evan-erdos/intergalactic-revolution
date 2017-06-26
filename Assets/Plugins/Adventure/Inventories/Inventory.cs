/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-22 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Inventories {
    public class Inventory<T> : Thing, IEnumerable<T> where T : IItem {
        public bool IsSynchronized => false;
        public bool IsReadOnly => false;
        public int Count => list.Count;
        public List<T> list = new List<T>();
        public T this[int index] { get { return list[index]; } set { list[index] = value; } }
        public void Add(IEnumerable<T> a) => list.Add(a);
        public void Add(T[] a) => list.Add(a.Cast<T>());
        public int IndexOf(T o) => list.IndexOf(o);
        public void Insert(int index, T o) => list.Insert(index,o);
        public void RemoveAt(int index) => list.RemoveAt(index);
        // public U GetItem<U>() where U : T => GetItems<T>().FirstOrDefault();
        // public List<U> GetItems<U>() where U : T => list.Where(o => o as T).Cast<U>().ToList();
        public void Add(T o) => list.Add(o);
        public void Clear() { list.ForEach(o => o.Drop()); list.Clear(); }
        public bool Contains(T o) => list.Contains(o);
        public void CopyTo(T[] arr, int n) => list.CopyTo(arr,n);
        public bool Remove(T o) => list.Remove(o);
        public void ForEach(Action<T> func) => list.ForEach(func);
        // public void ForEach<U>(Action<U> func) where U : T => list.Where(o => o is T).Cast<U>().ForEach(func);
        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator() as IEnumerator;

        new public class Data : Thing.Data {
            public decimal? cost {get;set;}
            public float mass {get;set;}
            public override Object Deserialize(Object o) {
                var instance = base.Deserialize(o) as Thing;
                return instance;
            }
        }
    }
}
