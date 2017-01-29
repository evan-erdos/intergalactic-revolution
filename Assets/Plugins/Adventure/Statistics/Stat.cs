/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-11-13 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Adventure.Statistics {

    /// Stat : object
    /// Base class of a statistic. Can perform Checks and can be used
    /// to process a Hit or some other roll / event based on statistics.
    public class Stat<T> {
        public StatKind kind;
        public bool Check() => true;
        public bool Check(Stat<T> stat) => true;
        protected T value {get;set;}
        public Stat() { }
        public Stat(StatKind kind) { this.kind = kind; }
        public Stat(StatKind kind, T value) : this(kind) { this.value = value; }
        public bool Fits(string s) => kind.ToString()==s;
        public bool Fits(Type type) => kind.ToString()==type.ToString();
    }

    public class StatSet<T> : Stat<T> {
        List<Stat<T>> stats = new List<Stat<T>>();
        public bool IsSynchronized => false;
        public bool IsReadOnly => false;
        public int Count => stats.Count;
        public object SyncRoot => stats;
        public StatSet() { }
        public StatSet(Stat<T>[] stats) { this.stats.AddRange(stats); }
        public StatSet(List<Stat<T>> stats) { this.stats.AddRange(stats); }
        public Stat<T> this[string stat] => stats.First(o => o.Fits(stat));
        public Stat<T> this[Type type] => stats.First(o => o.Fits(type));
        public void CopyTo(Stat<T>[] a, int n) => stats.CopyTo(a, n);
        public IEnumerator GetEnumerator() => stats.GetEnumerator();
    }

    public class HealthStats : StatSet<int> {
        Faculties faculties {get;set;}
        Condition condition {get;set;}
        public void AddCondition(Condition cond) { }
        public void AddConditions(params Condition[] conds) =>
            conds.ToList().ForEach(o => AddCondition(o));
    }
}
