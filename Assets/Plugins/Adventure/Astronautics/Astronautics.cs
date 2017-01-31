/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure.Astronautics {

    /// FlightMode
    /// defines the different kinds of flight control systems
    public enum FlightMode { Navigation, Assisted, Manual, Manuevering }

    /// SpaceAction : (sender, args) => void
    /// ubiquitous event handler, with a sender and event arguments
    public delegate void SpaceAction(ISpaceObject sender, SpaceArgs args);

    /// DamageAction : (sender, damage) => void
    /// handler for when stuff in space get damaged
    public delegate void DamageAction(ISpaceObject sender, float damage);

    /// SpaceObject : BaseObject
    /// provides a root class for every object in the namespace
    public abstract class SpaceObject : Adventure.BaseObject, ISpaceObject { }

    /// SpaceArgs : EventArgs
    /// provides a base argument type for space events
    public class SpaceArgs : EventArgs { }

    /// SpaceEvent : UnityEvent
    /// a serializable event handler to expose to the editor
    [Serializable] public class SpaceEvent : UnityEvent<ISpaceObject,SpaceArgs> { }

    /// SpaceError : error
    /// when something goes awry... in space!
    public class SpaceError : Exception { public SpaceError(string s) : base(s) { } }

    /// Astronomy
    /// contains relevant measurements of spacetime, plus discrete unit bases
    public static class Astronomy {
        public const float Day = (float) 86400; // seconds
        public const float km = (float) 0.001; // meters
        public const float kg = (float) 0.001; // tonnes
        public const float AU = (float) 149_597_870_700.0; // km
        public const float pc = (float) 206_265.0; // AU
        public const float Mass = (float) 1.98892e27; // tons
        public static float Time => (float) (Date-Epoch).TotalDays; // days
        public static DateTime Epoch => new DateTime(1994,10,20); // birthday
        public static DateTime Date = new DateTime(2017,1,20); // apocalypse
    }
}
