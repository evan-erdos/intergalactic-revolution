/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-11-18 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Puzzles {
    public abstract class Puzzle<T,U> : Piece<T,U>, IEnumerable<IPiece<T,U>> {
        [SerializeField] protected List<IPiece<T,U>> nearby = new List<IPiece<T,U>>();
        public bool IsReadOnly => false;
        public int Count => Pieces.Count;
        public HashSet<IPiece<T,U>> Pieces {get;} = new HashSet<IPiece<T,U>>();
        public override bool IsSolved => Pieces.Aggregate(true, (o,e) =>
            comparer.Equals(e.Solve(e.Condition), e.Solution));

        public override U Solve(T cond) => Pieces.Aggregate(default (U), (o,e) => e.Solve(e.Condition));
        public void Add(IPiece<T,U> o) => Pieces.Add(o);
        public void Clear() => Pieces.Clear();
        public void CopyTo(IPiece<T,U>[] a, int n) => Pieces.CopyTo(a,n);
        public bool Contains(IPiece<T,U> o) => Pieces.Contains(o);
        public bool Remove(IPiece<T,U> o) => Pieces.Remove(o);
        IEnumerator IEnumerable.GetEnumerator() => Pieces.GetEnumerator();
        public IEnumerator<IPiece<T,U>> GetEnumerator() => Pieces.GetEnumerator();

        protected override void Awake() { base.Awake();
            var list = new List<IPiece<T,U>>();
            foreach (Transform child in transform) {
                var children = child.gameObject.GetComponents<Thing>();
                if (children==null || children.Length<=0) continue;
                foreach (var o in children) if (o is IPiece<T,U> piece) list.Add(piece);
            }
        }
    }
}
