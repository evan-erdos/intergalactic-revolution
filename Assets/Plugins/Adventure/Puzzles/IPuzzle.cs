/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-11-18 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Puzzles {

    /// IPuzzle : IPiece, IPiece[]
    /// if a set of pieces is solved in a particular configuration,
    /// at which a point the puzzle is considered solved
    public interface IPuzzle<T,U> : IPiece<T,U>, IEnumerable<IPiece<T,U>> {

        /// Pieces : { IPiece<T> -> U }
        /// a mapping from pieces to the puzzle's solution,
        /// denoting the solved or unsolved state of the puzzle
        Map<IPiece<T,U>,U> Pieces {get;}

        /// Pose : (piece) => U
        /// iterates the puzzle, attempting to solve a particular piece,
        /// which could be one piece or a whole collection of pieces,
        /// depending on how the puzzle defines it's enumerator.
        U Pose(IPiece<T,U> piece);
    }
}
