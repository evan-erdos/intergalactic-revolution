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
        public ItemSet(List<Item> items) : base() { list.AddRange(items); }
        public Item this[int index] {
            get { return list[index]; }
            set { list[index] = value; } }
        public void Add<T>(T[] a) where T : Item => list.AddRange(a.Cast<Item>());
        public void Add<T>(IEnumerable<T> a) where T : Item =>
            list.AddRange(a.Where(item => item as Item).Cast<Item>());
        public int IndexOf(Item item) => list.IndexOf(item);
        public void Insert(int index, Item item) => list.Insert(index, item);
        public void RemoveAt(int index) => list.RemoveAt(index);
        public T GetItem<T>() where T : Item => GetItems<T>().FirstOrDefault();
        public List<T> GetItems<T>() where T : Item =>
            list.Where(item => item as T).Cast<T>().ToList();
        public void ForEach(Action<Item> func) => list.ForEach(func);
        public void ForEach<T>(Action<T> func) where T : Item =>
            list.Where(item => item is T).Cast<T>().ToList().ForEach(func);
        public void Add(Item item) => list.Add(item);
        public void Clear() { list.ForEach(item => item.Drop()); list.Clear(); }
        public bool Contains(Item item) => list.Contains(item);
        public void CopyTo(Item[] arr, int n) => list.CopyTo(arr,n);
        public bool Remove(Item item) => list.Remove(item);
        public IEnumerator<Item> GetEnumerator() => list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) list.GetEnumerator();
    }

    class ItemGroup<T> : Item, IItemGroup<T> where T : Item {
        public int Count {
            get { return count; }
            set { count = (value>0)?value:0; }
        } int count;

        public void Group() { }
        public IItemGroup<T> Split(int n) { Count -= n; return default (ItemGroup<T>); }
        public void Add(T elem) { Count++; Destroy(elem.gameObject); }
        public void Add(ItemGroup<T> elem) {
            if (elem.GetType()!=typeof(T)) return;
            Count += elem.Count;
            Destroy(elem.gameObject);
        }
    }
}
