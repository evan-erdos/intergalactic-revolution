/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-11-18 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Puzzles {
    public abstract class Puzzle<T> : Piece<T>, IEnumerable<IPiece<T>> {
        [SerializeField] protected List<IPiece<T>> externalPieces;
        [SerializeField] protected List<T> solveState;
        public bool IsReadOnly => false;
        public int Count => Pieces.Count;
        public Map<IPiece<T>,T> Pieces {get;} = new Map<IPiece<T>,T>();
        public override bool IsSolved =>
            Pieces.Aggregate(true, (total, next) =>
                EqualityComparer<T>.Default.Equals(
                    next.Key.Condition, next.Value));

        public override bool Solve(T condition) =>
            Pieces.Aggregate(true, (total,next) =>
                EqualityComparer<T>.Equals(
                    next.Key.Solve(next.Value), next.Value));

        public void Add(IPiece<T> o) => Pieces[o] = default (T);
        public void Clear() => Pieces.Clear();
        public bool Contains(IPiece<T> o) => Pieces.ContainsKey(o);
        public void CopyTo(IPiece<T>[] a, int n) => Pieces.Keys.CopyTo(a,n);
        public bool Remove(IPiece<T> o) => Pieces.Remove(o);
        IEnumerator IEnumerable.GetEnumerator() => Pieces.GetEnumerator();
        public IEnumerator<IPiece<T>> GetEnumerator() =>
            Pieces.GetEnumerator() as IEnumerator<IPiece<T>>;

        protected override void Awake() { base.Awake();
            var list = new List<IPiece<T>>();
            foreach (Transform child in transform) {
                var children = child.gameObject.GetComponents<Thing>();
                if (children==null || children.Length<=0) continue;
                foreach (var elem in children)
                    if (elem is IPiece<T> piece) list.Add(piece);
            } list.ForEach(item => Pieces[item] = solveState[list.IndexOf(item)]);
        }
    }
}
