/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-07-07 */

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;


namespace Adventure.Puzzles {


    /// IPiece : IObject
    /// a part of a puzzle which determines on its own if it's solved
    public interface IPiece : IObject {

        /// IsSolved : bool
        /// whether or not the current condition is the solution
        bool IsSolved {get;}

        /// SolveEvent : event
        /// raised when the piece is solved or becomes unsolved
        event StoryAction SolveEvent;

        /// Solve : () => void
        /// attempts to solve piece using current condition
        void Solve();
    }


    public interface IPiece<T,U> : IPiece {

        /// PoseEvent : event
        /// the pose event is raised anytime a change is made to the piece
        event PuzzleAction<T,U> PoseEvent;

        /// Condition : T
        /// the current configuration of the object
        T Condition {get;}

        /// Solution : T
        /// if the configuration of an instance is equal to its solution
        U Solution {get;}

        /// Pose : () => T
        /// applies some sort of transformation to the piece,
        /// and returns a state which can be used in a solve attempt
        T Pose();

        /// Solve : (T) => U
        /// the action of solving might represent the pull of a lever,
        /// or the placement of a piece in an actual jigsaw puzzle
        U Solve(T cond);
    }
}
