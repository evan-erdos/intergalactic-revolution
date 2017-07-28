/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-12-06 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Adventure.Inventories {


    /// ItemSet : Item[]
    /// A set of Items which deals with grouping,
    /// and can perform a really fast search on the basis of the
    /// possibly different and usually quite varied subtypes for
    /// easy filtering of specific types of Items.
    public class ItemSet : Item, IList<Item>, IItemSet {
        public bool IsSynchronized => false;
        public bool IsReadOnly => false;
        public int Count => list.Count;
        public List<Item> list = new List<Item>();
        public ItemSet() : base() { }
        public ItemSet(List<Item> items) : base() { list.Add(items); }
        public Item this[int i] { get { return list[i]; } set { list[i] = value; } }
        public void Add<T>(T[] a) where T : Item => list.Add(a.Cast<Item>());
        public void Add<T>(IEnumerable<T> a) where T : Item => list.Add(a.Where(o => o as Item).Cast<Item>());
        public int IndexOf(Item o) => list.IndexOf(o);
        public void Insert(int index, Item o) => list.Insert(index, o);
        public void RemoveAt(int index) => list.RemoveAt(index);
        public T GetItem<T>() where T : Item => GetItems<T>().FirstOrDefault();
        public List<T> GetItems<T>() where T : Item => list.Where(o => o as T).Cast<T>().ToList();
        public void ForEach(Action<Item> f) => list.ForEach(f);
        public void ForEach<T>(Action<T> f) where T : Item => list.Where(o => o is T).Cast<T>().ToList().ForEach(f);
        public void Add(Item o) => list.Add(o);
        public void Clear() { list.ForEach(o => o.Drop()); list.Clear(); }
        public bool Contains(Item o) => list.Contains(o);
        public void CopyTo(Item[] a, int n) => list.CopyTo(a,n);
        public bool Remove(Item o) => list.Remove(o);
        public IEnumerator<Item> GetEnumerator() => list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator() as IEnumerator;
    }


    class ItemGroup<T> : Item, IItemGroup<T> where T : Item {
        public int Count { get { return c; } set { c = (value>0)?value:0; } } int c;
        public void Group() { }
        public IItemGroup<T> Split(int n) { Count -= n; return default (ItemGroup<T>); }
        public void Add(T o) { Count++; Destroy(o.gameObject); }
        public void Add(ItemGroup<T> o) { if (o.GetType()==typeof(T)) { Count += o.Count; Destroy(o.gameObject); } }
    }
}
