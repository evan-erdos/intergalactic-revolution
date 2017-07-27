/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-11-13 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Inventories {


    /// IItem : IThing
    /// represents anything that can be picked up, dropped, and examined,
    /// but does not apply to other interactive things which don't move
    public interface IItem : IThing, IUsable {


        /// Held : bool
        /// is the item held in inventory?
        bool Held {get;}


        /// Mass : real
        /// the physical mass of the item
        float Mass {get;}


        /// TakeEvent : event
        /// raised when the item is taken
        event AdventureAction<StoryArgs> TakeEvent;


        /// DropEvent : event
        /// raised when the item is dropped
        event AdventureAction<StoryArgs> DropEvent;


        /// Take : () => void
        /// called when the item has been picked up
        void Take(StoryArgs e=null);


        /// Drop : () => void
        /// called when the item has been put back down
        void Drop(StoryArgs e=null);

    }
}
