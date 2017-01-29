/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-11-02 */

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Adventure.Inventories;

namespace Adventure {

    public class Settings {
        public string Name {get;set;} = "Adventure";
        public string Date {get;set;} = "2016-12-10";
        public string Release {get;set;} = "v0.2.1";
        public string Author {get;set;} = "Ben Scott";
        public string Handle {get;set;} = "@evan-erdos";
        public string Email {get;set;} = "bescott@andrew.cmu.edu";
        public string Link {get;set;} = "bescott.org/adventure/";
    }

    public delegate bool Cond();
    public delegate bool Cond<T>(T args);
    public delegate void Lambda();

    /// RealAction : (real) => void
    /// handler for signaling any change in a number [0...1]
    public delegate void RealAction(float value);

    /// RealEvent : UnityEvent
    /// a serializable event handler to expose to the editor
    [Serializable] public class RealEvent : UnityEvent<float> { }

    /// StoryAction : (thing,args) => void
    /// The standard event delegate for commands
    /// - thing : IThing
    ///     the thing which is issuing the command
    /// - args : StoryArgs
    ///     standard parse event arguments
    public delegate void StoryAction(IThing thing, StoryArgs args);

    [Serializable] public class StoryEvent : UnityEvent<IThing,StoryArgs> { }


    /// StoryException : Exception
    /// throw when anything is not well-formed, sensible, or reasonable
    class StoryException : Exception {
        public const string defaultMessage = "Something has gone horribly wrong.";
        internal StoryException(string message=defaultMessage) : base(message) { }
        internal StoryException(string message, Exception e) : base(message,e) { }
    }


    /// AmbiguityException : StoryException
    /// thrown when a command is not specific enough
    class AmbiguityException : StoryException {
        internal IList<IThing> options = new List<IThing>();
        internal AmbiguityException() : this("Be more specific.") { }
        internal AmbiguityException(string message, params IThing[] options)
            : this(message,new List<IThing>(options)) { }
        internal AmbiguityException(string message, IEnumerable<IThing> options)
            : base(message) { this.options = new List<IThing>(options); }
    }


    /// MoralityException : StoryException
    /// throw in response to any manner of moral turpitude
    class MoralityException : StoryException {
        internal Cond cond {get;set;}
        internal StoryAction then {get;set;}
        internal MoralityException(StoryAction then) : this("", then) { }
        internal MoralityException(string message, StoryAction then)
            : this(message, then, () => false) { }
        internal MoralityException(string message, StoryAction then, Cond cond)
            : base(message, new StoryException()) {
                (this.cond, this.then) = (cond,then); }
    }


    /// IUsable : interface
    /// things which can be used, the base for all verb-ables
    public interface IUsable : IThing {

        /// Use : () => void
        /// use the thing
        void Use();
    }

    /// IOpenable : interface
    /// anything that can be opened or closed
    public interface IOpenable : IUsable {

        /// Open : () => void
        /// called when the object needs to be opened,
        /// and returns a boolean value denoting if it was
        void Open();

        /// Shut : () => void
        /// called when the object needs to be closed,
        /// and returns a boolean value denoting if it was
        void Shut();
    }

    /// ILockable : interface
    /// anything which can be locked or unlocked,
    /// also specifies a Key object to unlock with
    public interface ILockable : IUsable {

        /// IsLocked : bool
        /// whether or not the object is locked
        bool IsLocked {get;}

        /// KeyMatch : Key
        /// a Key object to be checked against when unlocking
        Key LockKey {get;}

        /// Lock : (thing) => bool
        /// called when an attempt is made to lock the object
        /// - thing : Key
        ///     optional key to use to lock the object
        void Lock(Thing thing);

        /// Unlock : (thing) => bool
        /// called when an attempt is made to unlock the object
        /// - thing : Key
        ///     optional key to use to unlock the object
        void Unlock(Thing thing);
    }

    /// ISurface : interface
    /// anything which can have things put on it
    public interface ISurface : IUsable {

        /// Put : (thing) => void
        /// called when something should be put on something
        void Put(Thing thing);
    }

    /// IPushable : interface
    /// anything which can be pushed
    public interface IPushable : IUsable {

        /// Push : () => void
        /// called when the object should be pushed
        void Push();

        /// Pull : () => void
        /// called when the object should be pulled
        void Pull();
    }

    /// IReadable : interface
    /// Interface to anything that can be read.
    public interface IReadable : IUsable {

        /// Passage : string
        /// Represents the body of text to be read
        string Passage {get;}

        /// Read : () => void
        /// Function to call when reading something.
        void Read();
    }


    namespace Puzzles {

        /// PuzzleArgs : StoryArgs
        /// encapsulates the most important part of a puzzle: is it solved?
        public class PuzzleArgs : StoryArgs {
            public bool IsSolved {get;set;}
            public PuzzleArgs(bool IsSolved) { this.IsSolved = IsSolved; }
        }

        /// PuzzleAction :  event
        /// when a piece is posed, its parent should be notified via this event
        /// - piece : T
        ///     the IPiece<T> sending this event
        /// - args : PuzzleArgs
        ///     ubiquitous event arguments
        public delegate void PuzzleAction<T>(IPiece<T> piece, PuzzleArgs args);

        [Serializable]
        public class PuzzleEvent<T> : UnityEvent<IPiece<T>,PuzzleArgs> { }
    }


    namespace Inventories {

        /// Keys : enum
        /// enumerates the ranks of LockKey that can be used
        public enum Keys { Default, Breaker, Radial, Master, Skeleton, Unique }

        /// IItemGroup<T> : IItem
        /// manages groups of Items as a single instance
        public interface IItemGroup<T> : IItem where T : IItem {

            /// Count : int
            /// number of items in this group
            int Count {get;set;}

            /// Group : () => void
            /// causes the item set to regroup
            void Group();

            /// Split : (int) => IItemGroup<T>
            /// splits a number of items away from the group and return them
            IItemGroup<T> Split(int n);
        }

        /// IGainful : IItem
        /// represents objects that have value, and can be traded
        public interface IGainful : IItem {

            /// Cost : decimal
            /// monetary price of the item
            decimal Cost {get;}

            /// Buy : () => void
            /// purchases the item
            void Buy();

            /// Sell : () => void
            /// sells the item
            void Sell();
        }

        /// IWearable : IWearable
        /// represents anything that can be worn
        public interface IWearable : IItem {

            /// Worn : bool
            /// is the object currently being worn?
            bool Worn {get;}

            /// Wear : () => void
            /// equips the object
            void Wear();

            /// Stow : () => void
            /// puts away the object
            void Stow();
        }

        /// IWieldable : IWearable
        /// represents anything that can be used to attack someone
        public interface IWieldable : IWearable {

            /// Grip : Transform
            /// the spot on the object to grab in world coordinates
            Transform Grip {get;}

            /// Attack : () => void
            /// called when the object is being used to attack
            void Attack();
        }

        /// IItemSet : interface
        /// defines a set of items which can be iterated over and queried
        public interface IItemSet : IList<Item> {

            void Add<T>(params T[] items) where T : Item;

            void Add<T>(IEnumerable<T> items) where T : Item;

            /// GetItems : () => T[]
            /// gets all items in the set whose type matches the parameter
            List<T> GetItems<T>() where T : Item;

            /// GetItem : () => T
            /// gets an item whose type matches the parameter
            T GetItem<T>() where T : Item;
        }
    }


    namespace Statistics {

    /// Damages : enum
    /// the many varieties of damaging effects:
    /// - Default: direct damage, factoring in no resistances
    /// - Pierce: penetrative damage, applies to sharp and very fast things
    /// - Crush: brute force damage, usually as a result of very heavy impacts
    /// - Fire: burning damage, spreads and melts things
    /// - Ice: cold damage, slows and freezes things
    /// - Magic: magical damage, just like you expect
    public enum Damages { Default, Pierce, Crush, Fire, Ice, Magic }

    /// Hits : enum
    /// the many types of hits:
    /// - Default: baseline affinity, always carries out the attack
    /// - Miss: no contact at all, attack action not taken
    /// - Graze: a glancing blow, or extremely ineffective hit
    /// - Hit: default hit, normal effectiveness and calculations
    /// - Crit: a critical hit, very damaging / extremely effective
    public enum Hits { Default, Miss, Graze, Hit, Crit }

    public enum StatKind {
        Health, Endurance, Strength, Agility,
        Dexterity, Perception, Intellect, Memory }

    public enum Affinities { Default, Miss, Graze, Hit, Crit }
    [Flags] public enum Faculties { None, Thinking, Breathing, Sensing, Moving }

    [Flags] public enum Condition {
        None, Unknown, Default, Polytrauma,
        Dead, Maimed, Wounded, Injured,
        Scorched, Burned, Frozen, Poisoned,
        Paralysis, Necrosis, Infection, Fracture,
        Ligamentitis, Radiation, Poisioning, Hemorrhage,
        Frostbite, Thermosis, Hypothermia, Hyperthermia,
        Hypohydratia, Inanition, Psychosis, Depression,
        Psychotic, Shocked, Stunned, Healthy }
    }
}
