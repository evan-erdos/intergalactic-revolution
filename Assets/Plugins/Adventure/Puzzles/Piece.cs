/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-11-18 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure.Puzzles {

    public abstract class Piece : Thing, IPiece {
        [SerializeField] protected StoryEvent onSolve = new StoryEvent();
        public virtual event AdventureAction<StoryArgs> SolveEvent;
        public virtual bool IsSolved => true;
        public virtual void Use() => Solve();
        public virtual void Solve(StoryArgs e=null) => SolveEvent(e ?? new StoryArgs { Sender=this });
        async void OnSolve() { Log($"{Name} solved!"); await 1; }
        public override void Init() { base.Init(); SolveEvent += e => onSolve?.Call(e); }
    }

    public abstract class Piece<T,U> : Piece, IPiece<T,U> {
        protected EqualityComparer<U> comparer = EqualityComparer<U>.Default;
        [SerializeField] protected Event<PuzzleArgs<T,U>> onPose;
        public event AdventureAction<PuzzleArgs<T,U>> PoseEvent;
        public override bool IsSolved => comparer.Equals(Solver(Condition),Solution);
        public virtual T Condition {get;protected set;}
        public virtual U Solution {get;protected set;}
        public virtual Func<T,U> Solver {get;protected set;}
        public abstract T Pose(PuzzleArgs<T,U> e=null);
        public abstract U Solve(T cond);
        async void OnPose() { Log($"{Name} posed!"); await 0; }
        public override void Init() { base.Init(); onPose += e => OnPose(); }

        new public class Data : Thing.Data {
            public T condition {get;set;}
            public U solution {get;set;}
            public override Object Deserialize(Object o) {
                var instance = base.Deserialize(o) as Piece<T,U>;
                if (instance is null) return default(Object);
                (instance.Condition, instance.Solution) = (condition,solution);
                return instance;
            }
        }
    }
}
