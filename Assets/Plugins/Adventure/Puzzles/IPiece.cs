/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-11-18 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure.Puzzles {

    public interface IPiece {

        /// SolveEvent : event
        /// notify subscribers in the event that they are solved
        event StoryAction SolveEvent;

        /// IsSolved : bool
        /// whether or not the current state is the solution
        /// - ensure : IsSolved==(Condition==Solution)
        bool IsSolved {get;}

        /// Solve : () => bool
        /// does not attempt a new solution, but simply checks the exiting one
        bool Solve();

    }

    /// IPiece<T> : interface
    /// An element of a larger puzzle which can change the puzzle,
    /// based on its own status as solved, or unsolved.
    /// In the case of more complicated base types,
    /// it could represent a digit on a combination lock.
    /// In that case, a given piece might not have its own solution,
    /// but could represent a solved puzzle when considered in aggregate.
    public interface IPiece<T> {

        /// PoseEvent : event
        /// notify subscribers in the event that they are solved
        event PuzzleAction<T> PoseEvent;

        /// IsSolved : bool
        /// whether or not the current state is the solution
        /// - ensure : IsSolved==(Condition==Solution)
        bool IsSolved {get;}

        /// Condition : T
        /// the current configuration
        T Condition {get;}

        /// Solution : T
        /// if the configuration of an instance is equal to its solution
        T Solution {get;}

        /// Pose : () => T
        /// applies some sort of transformation to the piece,
        /// and returns a state which can be used in a solve attempt
        T Pose();

        /// Solve : (T) => bool
        /// the action of solving might represent the pull of a lever,
        /// or the placement of a piece in an actual jigsaw puzzle
        /// - condition : T
        /// value to attempt to solve with
        bool Solve(T condition);
    }
}
