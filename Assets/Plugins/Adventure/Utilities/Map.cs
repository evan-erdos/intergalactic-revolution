/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-11-01 */

using System;
using System.Collections;
using System.Collections.Generic;

/// Map<T> : Dictionary<string,T>
/// a simple wrapper for Dictionary which drastically shortens the name for maps
public class Map<T> : Dictionary<string,T> { }
public class Map<K,V> : Dictionary<K,V> { }

/// TypeMap<T> : (type) -> Func<T>
/// Maps from types (T and subclasses thereof)
/// to instances whose type takes the type they're keyed to as a parameter
public class TypeMap<T> : Map<Type,List<Func<T>>> {
    Map<Type,List<Func<T>>> map = new Map<Type,List<Func<T>>>();
    public new List<Func<T>> this[Type type] {
        get { return map[type]; }
        set { map[type] = value; }}
    public List<Func<T>> Get<U>() where U : T => map[typeof(U)];
    public void Set<U>(List<Func<T>> value) where U : T =>
        map[typeof(U)] = (List<Func<T>>) value;
}

/// RandList<T> : List<T>
/// A simple wrapper class for lists which returns a random element
public class RandList<T> : List<T> {
    Random random = new Random();
    public T Next() => (Count==0) ? default(T) : this[random.Next(Count)];
}

/// IterList<T> : List<T>
/// A simple wrapper for lists which simply steps through the lis
public class IterList<T> : List<T> {
    int Current = -1;
    public T Next() => (Count==0 || Current>Count) ? default(T) : this[++Current];
}

/// LoopList<T> : List<T>
/// A simple wrapper class for List<T>, which adds the
/// ability to return a random element from the list.
public class LoopList<T> : List<T> {
    int Current = -1;
    public T Next() => (Count==0) ? default(T) : this[++Current%Count];
}
