/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-11-18 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure.Puzzles {

    public abstract class Piece : Thing, IPiece {
        [SerializeField] protected StoryEvent onSolve = new StoryEvent();
        public virtual event StoryAction SolveEvent;
        public virtual bool IsSolved => true;
        public virtual bool Solve() => IsSolved;
        public virtual void Use() => Solve();
        //public override bool Solve() => SolveEvent(null,new PuzzleArgs(IsSolved));

        protected virtual void OnSolve() {
            StartSemaphore(Solving);
            IEnumerator Solving() {
                Log($"{Name} solved!");
                yield return new WaitForSeconds(1);
            }
        }

        protected override void Awake() { base.Awake();
            onSolve?.AddListener((o,e) => OnSolve());
            SolveEvent += (o,e) => onSolve?.Invoke(o,e);
        }
    }

    public abstract class Piece<T> : Piece, IPiece<T> {
        protected EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        [SerializeField] protected PuzzleEvent<T> onPose = new PuzzleEvent<T>();
        public virtual event PuzzleAction<T> PoseEvent;
        public override bool IsSolved => comparer.Equals(Condition,Solution);
        public virtual T Condition {get;protected set;}
        public virtual T Solution {get;protected set;}
        public abstract T Pose();
        public abstract bool Solve(T condition);

        protected virtual void OnPose() {
            StartSemaphore(Posing);
            IEnumerator Posing() { Log($"{Name} posed!"); yield break; }
        }

        protected override void Awake() { base.Awake();
            onPose?.AddListener((o,e) => OnPose());
            PoseEvent += (o,e) => onPose?.Invoke(o,e);
        }

        new public class Data : Thing.Data {
            public T condition {get;set;}
            public T solution {get;set;}
            public override BaseObject Deserialize(BaseObject o) {
                var instance = base.Deserialize(o) as Piece<T>;
                if (!instance) return default(BaseObject);
                instance.Condition = this.condition;
                instance.Solution = this.solution;
                return instance;
            }
        }
    }
}
