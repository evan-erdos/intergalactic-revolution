/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-11-18 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Puzzles {

    /// IPuzzle : IPiece, IEnumerable<IPiece>
    /// if a set of pieces is solved in a particular configuration,
    /// at which a point the puzzle is considered solved
    public interface IPuzzle<T> : IPiece<T>, IEnumerable<IPiece<T>> {

        /// Pieces : { IPiece<T> -> T }
        /// a mapping from pieces to the puzzle's solution,
        /// denoting the solved or unsolved state of the puzzle
        Map<IPiece<T>,T> Pieces {get;}

        /// Pose : (piece) => void
        /// iterates the puzzle, attempting to solve a particular piece,
        /// which could be one piece or a whole collection of pieces,
        /// depending on how the puzzle defines it's enumerator.
        T Pose(IPiece<T> piece);
    }
}
