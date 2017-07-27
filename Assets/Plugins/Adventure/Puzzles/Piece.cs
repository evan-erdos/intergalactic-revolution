/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-11-18 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure.Puzzles {

    public abstract class Piece : Thing, IPiece {
        [SerializeField] protected Event<StoryArgs> onSolve = new Event<StoryArgs>();
        public virtual event AdventureAction<StoryArgs> SolveEvent;
        public virtual bool IsSolved => true;
        public virtual void Solve(StoryArgs e=null) => SolveEvent(e ?? new StoryArgs { Sender = this });
        public virtual void Use() => Solve();
        async void OnSolve() { Log($"{Name} solved!"); await 1; }
        public override void Init() { base.Init(); SolveEvent += e => OnSolve(); }
    }

    public abstract class Piece<T,U> : Piece, IPiece<T,U> {
        protected EqualityComparer<U> comparer = EqualityComparer<U>.Default;
        [SerializeField] protected Event<PuzzleArgs<T,U>> onPose = new Event<PuzzleArgs<T,U>>();
        public event AdventureAction<PuzzleArgs<T,U>> PoseEvent {add{onPose.Add(value);} remove{onPose.Remove(value);}}
        public override bool IsSolved => comparer.Equals(Solver(Condition),Solution);
        public virtual T Condition {get;protected set;}
        public virtual U Solution {get;protected set;}
        public virtual Func<T,U> Solver {get;protected set;}
        public abstract T Pose(PuzzleArgs<T,U> e=null);
        public abstract U Solve(T cond);
        public override void Init() { base.Init(); PoseEvent += e =>  OnPose(); }
        async void OnPose() { Log($"{Name} posed!"); await 0; }

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
