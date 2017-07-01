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


    /// Settings
    /// simple class for program-wide constants and configurations
    public class Settings {
        public string Name {get;set;} = "Adventure";
        public string Date {get;set;} = "2017-06-26";
        public string Version {get;set;} = "v0.3.1";
        public string Author {get;set;} = "Ben Scott";
        public string Handle {get;set;} = "@evan-erdos";
        public string Email {get;set;} = "bescott@andrew.cmu.edu";
        public string Link {get;set;} = "bescott.org/adventure/";
    }


    /// Styles : enum
    /// This enumerates the various formatting options that the
    /// Terminal can use. Most values have some meaning, which
    /// are used by the formatting function. They might
    /// be hex values for colors, sizes of headers, etc.
    public enum Styles {
        Inline=0, Newline=1, Paragraph=2, Refresh=3, Indent=4,
        h1=24, h2=18, h3=16, h4=14,
        Default=0xFFFFFF, State=0x2A98AA, Change=0xFFAE10,
        Alert=0xFC0000, Command=0xBBBBBB, Warning=0xFA2363,
        Help=0x9CDF91, Title=0x98C8FC, Static=0xFFDBBB
    }


    /// Lambda : () => void
    /// spirit animal of the action delegate from system namespace
    public delegate void Lambda();


    /// Cond : () => bool
    /// represents a function which checks the truth state of something
    public delegate bool Cond();
    public delegate bool Cond<T>(T args);


    /// RealAction : (real) => void
    /// handler for signaling any change in a number [0...1]
    public delegate void RealAction(float value);


    /// RealityAction : event
    /// base event delegate for movement of a tracked object
    public delegate void RealityAction(IObject o, RealityArgs e);


    /// MovementAction : event
    /// base event delegate for movement of a tracked object
    public delegate void MovementAction(IObject o, MovementArgs e);


    /// CombatAction : event
    /// base event delegate for movement of a tracked object
    public delegate void CombatAction(IObject o, CombatArgs e);


    /// ButtonAction : event
    /// base event delegate for clicking a binary button
    public delegate void ButtonAction(IObject o, ButtonArgs e);


    /// TouchpadAction : event
    /// base event delegate for clicking a binary button
    public delegate void TouchpadAction(IObject o, TouchpadArgs e);


    /// SliderAction : event
    /// base event delegate for clicking a binary button
    public delegate void SliderAction(IObject o, SliderArgs e);


    /// StoryAction : event
    /// The standard event delegate for commands
    public delegate void StoryAction(IThing thing, StoryArgs args);


    /// RealityArgs : EventArgs
    /// provides a base argument type for VR events
    public class RealityArgs : EventArgs { public Vector3 Position {get;set;} }


    /// MovementArgs : RealityArgs
    /// provides a base argument type for VR events
    public class MovementArgs : RealityArgs {
        public Vector3 Displacement {get;set;}
        public Vector3 Velocity {get;set;}
        public Vector3 Angular {get;set;} }


    /// CombatArgs : MovementArgs
    /// provides a base argument type for VR events
    public class CombatArgs : MovementArgs { public float Damage {get;set;} }


    /// ButtonArgs : EventArgs
    /// provides a base argument type for VR events
    public class ButtonArgs : MovementArgs {
        public bool IsPressed {get;set;}
        public bool IsReleased {get;set;}
        public bool IsHeld {get;set;} }


    /// SliderArgs : ButtonArgs
    /// provides a base argument type for real number sliders
    public class SliderArgs : ButtonArgs { public float Value {get;set;} }


    /// CursorArgs : EventArgs
    /// provides arguments for VR cursors
    public class CursorArgs : ButtonArgs { public IObject Target {get;set;} }


    /// TouchpadArgs : EventArgs
    /// provides a base argument type for VR events
    public class TouchpadArgs : ButtonArgs { public (float x, float y) TouchPosition {get;set;} }


    /// StoryArgs : EventArgs
    /// encapsulates the event data
    public class StoryArgs : System.EventArgs, Word {
        StoryAction Command {get;set;}
        public Verb Verb {get;set;}
        public Regex Pattern => Verb.Pattern;
        public string[] Grammar => Verb.Grammar;
        public string Input {get;set;}
        public string Message {get;set;}
        public IThing Goal {get;set;}
        public StoryArgs() : base() { }
        public StoryArgs(string message) { this.Message = message; }
        public StoryArgs(Verb verb,string input="",string message="") {
            (this.Verb, this.Input, this.Message) = (verb, input, message); } }


    /// serializable event handlers to expose to the editor
    [Serializable] public class RealEvent : UnityEvent<float> { }
    [Serializable] public class RealityEvent : UnityEvent<IObject,RealityArgs> { }
    [Serializable] public class MovementEvent : UnityEvent<IObject,MovementArgs> { }
    [Serializable] public class CombatEvent : UnityEvent<IObject,CombatArgs> { }
    [Serializable] public class ButtonEvent : UnityEvent<IObject,ButtonArgs> { }
    [Serializable] public class TouchpadEvent : UnityEvent<IObject,TouchpadArgs> { }
    [Serializable] public class SliderEvent : UnityEvent<IObject,SliderArgs> { }
    [Serializable] public class StoryEvent : UnityEvent<IThing,StoryArgs> { }


    /// Error : error
    /// base error for the entire namespace
    public class Error : Exception {
        public Error(string message, Exception error) : base(message,error) { }
        public Error(string message="What have you done?") : this(message, new Exception()) { } }

    /// StoryError : error
    /// throw when anything is not well-formed, sensible, or reasonable
    public class StoryError : Error {
        public StoryError(string message="What have you done?") : base(message, new Error()) { }
        public StoryError(string message, Error error) : base(message,error) { } }


    /// AmbiguityError : error
    /// thrown when a command is not specific enough
    public class AmbiguityError : StoryError {
        internal IList<IThing> options = new List<IThing>();
        internal AmbiguityError() : this("Be more specific.") { }
        internal AmbiguityError(string message, params IThing[] options)
            : this(message, new List<IThing>(options)) { }
        internal AmbiguityError(string message, IEnumerable<IThing> options)
            : base(message) { this.options = new List<IThing>(options); } }


    /// MoralityError : error
    /// throw in response to any manner of moral turpitude
    public class MoralityError : StoryError {
        internal Cond cond {get;set;}
        internal StoryAction then {get;set;}
        internal MoralityError(StoryAction then) : this("", then) { }
        internal MoralityError(string message, StoryAction then) : this(message, then, () => false) { }
        internal MoralityError(string message, StoryAction then, Cond cond)
            : base(message,new StoryError()) { (this.cond, this.then) = (cond, then); } }


    /// Noun : Word
    /// a kind of word which refers to a thing which can be acted on
    public struct Noun : Word {
        public Regex Pattern {get;set;}
        public string[] Grammar {get;set;}
        public Noun(Regex pattern, string[] grammar) {
            (this.Pattern, this.Grammar) = (pattern, grammar); } }


    /// Verb : Word
    /// any sort of word which correlates to a command
    public struct Verb : Word {
        public Regex Pattern {get;set;}
        public string[] Grammar {get;set;}
        public StoryAction Command {get;set;}
        public Verb(Regex pattern, string[] grammar, StoryAction command=null) {
            (this.Pattern, this.Grammar, this.Command) = (pattern, grammar, command); }
    }


    /// Message : ILoggable
    /// Represents any textual message that can be formatted and rendered
    public struct Message : ILoggable {
        public string Name {get;set;}
        public string Content {get;set;}
        public Message(string content="", string name="") : this() {
            (this.Name, this.Content) = (name, content); }
        public void Log() => Terminal.Log(Content.md());
        public override string ToString() => Content;
        public static implicit operator string(Message o) => o.Content.md();
    }


    /// IProportion
    /// anything which can be created from a single ratio, [0...1]
    public interface IProportion { float Ratio {get;set;} }


    /// Word : interface
    /// any variety of recognizable, textual pattern
    public interface Word {

        /// Pattern : /regex/
        /// the pattern to be used to disambiguate objects
        Regex Pattern {get;}

        /// Grammar : string
        /// discrete options used for processing object definitions
        string[] Grammar {get;}
    }


    /// ICreatable
    /// any behaviour prefab which can be created at runtime
    public interface ICreatable {

        /// CreateEvent : event
        /// event raised when the object is created
        event RealityAction CreateEvent;

        /// Init : () => void
        /// does local setup when creating an object
        void Init();

        /// Create : (o,e) => void
        /// calls the create event callback
        void Create();
        void Create(IObject o, RealityArgs e);
    }

    public interface ICreatable<T> : ICreatable {

        /// Create : (data) => serialized game object
        /// applies the data in the data object to the object
        void Create(T data);
    }


    /// ILoggable : interface
    /// anything which can be logged, descriptions, messages, etc
    public interface ILoggable {

        /// Content : string
        /// unformatted representation of this instance's description
        string Content {get;}

        /// Log : () => void
        /// event callback, returns the fully-formatted string to be displayed
        void Log();
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


        /// PuzzleEvent : UnityEvent
        /// a serializable event handler to expose to the editor
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


            /// Add : (T[]) => void
            /// adds all items in the array to the group
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
