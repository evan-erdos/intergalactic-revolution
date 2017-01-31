/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-11-02 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure {

    /// IThing : IDescribable
    /// provides a common interface to all things in the Adventure system
    public interface IThing : IObject, ILoggable {

        /// Range : real
        /// determines event ranges (sqrMagnitude, adjust accordingly)
        float Range {get;}

        /// Mask : LayerMask
        /// a mask containing all the layers the thing can interact with
        LayerMask Mask {get;}

        /// Location : Transform
        /// the location and context of the thing (transform parent)
        Transform Location {get;}

        /// ViewEvent : event
        /// raised when the thing is viewed
        event StoryAction ViewEvent;

        /// ViewEvent : event
        /// raised when the thing is viewed
        event StoryAction FindEvent;

        /// LogEvent : event
        /// raised when a message is to be logged
        event StoryAction LogEvent;

        /// Do : () => void
        /// invokes the default verb operation for the thing, e.g.,
        /// View for things, Drop for items, an open / shut toggle for doors
        void Do();

        /// Find : () => void
        /// attempts to find the thing, and notifies the find event
        /// - throw : TextException
        ///     the thing can't be found, isn't visible, or isn't nearby
        void Find();

        /// View : () => void
        /// attempts to view the thing, and notifies the view event
        /// - throw : TextException
        ///     the thing can't be found, or it can't be examined
        void View();
    }
}
