/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-11-02 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure {


    /// IThing : IDescribable
    /// provides a common interface to all interactable things
    public interface IThing : IObject, ILoggable {


        /// Mask : LayerMask
        /// a mask containing all the layers the thing can interact with
        LayerMask Mask {get;}


        /// Location : Transform
        /// the location and context of the thing (transform parent)
        Transform Location {get;}


        /// indexer : [string] => description
        /// allows all things to send descriptive information anywhere
        string this[string index] {get;}


        /// ViewEvent : event
        /// raised when the thing is viewed
        event AdventureAction<StoryArgs> ViewEvent;


        /// FindEvent : event
        /// raised when the thing is searched for
        event AdventureAction<StoryArgs> FindEvent;


        /// LogEvent : event
        /// raised when a message is to be logged
        event AdventureAction<StoryArgs> LogEvent;


        /// Do : () => void
        /// invokes the default verb operation for the thing, e.g.,
        /// View for things, Drop for items, an open / shut toggle for doors
        void Do();


        /// Find : () => void
        /// attempts to find the thing, and notifies the find event
        /// - throw : the thing can't be found, isn't visible, or isn't nearby
        void Find(StoryArgs e=null);


        /// View : () => void
        /// attempts to view the thing, and notifies the view event
        /// - throw : the thing can't be found, or it can't be examined
        void View(StoryArgs e=null);
    }
}
