/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-11-02 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure.Locales {

    /// IPath : IThing
    /// anything which can take damage and be killed
    public interface IPath : IThing {

        /// Destination : IPath
        /// the endpoint of this path
        Room Destination {get;}

        /// TravelEvent : event
        /// raised when the path is traversed
        event StoryAction TravelEvent;

        /// Travel : (path) => void
        /// moves the user it's called on to the target destination
        void Travel(Thing destination);
    }
}
