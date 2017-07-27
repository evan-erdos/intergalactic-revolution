/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-11-02 */

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Random=System.Random;
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
        public string Version {get;set;} = "0.3.2";
        public string Link {get;set;} = "bescott.org/adventure/";
        public string Author {get;set;} = "Ben Scott";
        public string Handle {get;set;} = "@evan-erdos";
        public string Email {get;set;} = "admin@bescott.org"; }


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
        Help=0x9CDF91, Title=0x98C8FC, Static=0xFFDBBB }


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


    /// AdventureAction : event
    /// base event delegate for events in adventures
    public delegate void AdventureAction<T>(T e=null) where T : AdventureArgs;


    /// AdventureArgs : EventArgs
    /// provides a base argument type for all event arguments in adventures
    public abstract class AdventureArgs : EventArgs { public IObject Sender {get;set;} }


    /// RealityArgs : AdventureArgs
    /// provides a base argument type for VR events
    public class RealityArgs : AdventureArgs { public Vector3 Position {get;set;} }


    /// MovementArgs : RealityArgs
    /// provides a base argument type for VR events
    public class MovementArgs : RealityArgs {
        public Vector3 Displacement {get;set;}
        public Vector3 Velocity {get;set;}
        public Vector3 Angular {get;set;} }


    /// FlightArgs : MovementArgs
    /// provides a base argument type for piloting spaceships
    public class FlightArgs : MovementArgs {
        public float Roll {get;set;}
        public float Pitch {get;set;}
        public float Yaw {get;set;}
        public float Thrust {get;set;}
        public float Lift {get;set;}
        public float Strafe {get;set;}
        public float Turbo {get;set;}
        public List<Vector3> Course {get;set;} }


    /// TravelArgs : MovementArgs
    /// provides a base argument type for moving between stars
    public class TravelArgs : MovementArgs { public SpobProfile Destination {get;set;} }


    /// CombatArgs : MovementArgs
    /// provides a base argument type for VR events
    public class CombatArgs : MovementArgs { public float Damage {get;set;} }


    /// AttackArgs : CombatArgs
    /// provides a base argument type for an attack made with a weapon
    public class AttackArgs : CombatArgs { public ITrackable Target {get;set;} }


    /// ButtonArgs : EventArgs
    /// provides a base argument type for VR events
    public class ButtonArgs : MovementArgs {
        public (bool IsDown, bool IsUp, bool IsHeld) Input {get;set;} }


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
    public class StoryArgs : AdventureArgs, Word {
        AdventureAction<StoryArgs> Command {get;set;}
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

    [Serializable] public class RealEvent : UnityEvent<float> { }

    /// Event : event
    /// base serializable event handler to expose events to editor
    [Serializable] public class Event<T> : UnityEvent<T> where T : AdventureArgs {
        public event AdventureAction<T> E {add{Remove(value);Add(value);} remove{Remove(value);}}
        public void Call(T e=null) => Invoke(e);
        public void Add(Event<T> e) => Add(e.Call);
        public void Add(AdventureAction<T> e) => AddListener(o => e(o));
        public void Remove(Event<T> e) => Remove(e.Call);
        public void Remove(AdventureAction<T> e) => RemoveListener(o => e(o)); }


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
        internal AdventureAction<StoryArgs> then {get;set;}
        internal MoralityError(AdventureAction<StoryArgs> then) : this("", then) { }
        internal MoralityError(string message, AdventureAction<StoryArgs> then) : this(message, then, () => false) { }
        internal MoralityError(string message, AdventureAction<StoryArgs> then, Cond cond)
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
        public AdventureAction<StoryArgs> Command {get;set;}
        public Verb(Regex pattern, string[] grammar, AdventureAction<StoryArgs> command=null) {
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


    /// IResettable : IObject
    /// an object which can be damaged and destroyed
    public interface IResettable {

        /// Reset : () => fixed
        /// puts everything back to normal
        void Reset();
    }


    /// ICreatable
    /// any behaviour prefab which can be created at runtime
    public interface ICreatable {

        /// CreateEvent : event
        /// event raised when the object is created
        event AdventureAction<RealityArgs> CreateEvent;

        /// Create : () => void
        /// does local setup when creating an object
        void Create(RealityArgs e=null);

        /// Init : () => void
        /// does local setup when creating an object
        void Init();
    }

    public interface ICreatable<T> : ICreatable {

        /// Create : (data) => serialized game object
        /// applies the data in the data object to the object
        void Create(T data);
    }

    /// ITrackable : IObject
    /// anything which moves around and has a velocity and a position
    public interface ITrackable : IObject {

        /// Velocity : (real,real,real)
        /// current rate of speed of the ship
        Vector3 Velocity {get;}
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

        /// OpenEvent : event
        /// notifies everyone that we're opening
        event AdventureAction<StoryArgs> OpenEvent;

        /// ShutEvent : event
        /// notifies everyone that we're opening
        event AdventureAction<StoryArgs> ShutEvent;

        /// Open : () => void
        /// called when the object needs to be opened,
        /// and returns a boolean value denoting if it was
        void Open(StoryArgs e=null);

        /// Shut : () => void
        /// called when the object needs to be closed,
        /// and returns a boolean value denoting if it was
        void Shut(StoryArgs e=null);
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

        /// ReadEvent: event
        /// notifies everybody that this has been read
        event AdventureAction<StoryArgs> ReadEvent;

        /// Read : () => void
        /// raises reading event while reading something
        void Read(StoryArgs e=null);
    }


    /// Set<T> : HashSet<string,T>
    /// a simple wrapper which makes the name for sets consistent
    public class Set<T> : HashSet<T> { }

    /// Map<T> : Dictionary<string,T>
    /// a simple wrapper for Dictionary which drastically shortens the name for maps
    public class Map<T> : Dictionary<string,T> { } public class Map<K,V> : Dictionary<K,V> { }

    /// TypeMap<T> : (type) -> Func<T>
    /// Maps from types (T and subclasses thereof)
    /// to instances whose type takes the type they're keyed to as a parameter
    public class TypeMap<T> : Map<Type,List<Func<T>>> {
        Map<Type,List<Func<T>>> map = new Map<Type,List<Func<T>>>();
        public new List<Func<T>> this[Type type] { get { return map[type]; } set { map[type] = value; }}
        public List<Func<T>> Get<U>() where U : T => map[typeof(U)];
        public void Set<U>(List<Func<T>> value) where U : T => map[typeof(U)] = (List<Func<T>>) value;
    }

    /// RandList<T> : List<T>
    /// A simple wrapper class for lists which returns a random element
    public class RandList<T> : List<T> {
        Random random = new Random(); public T Next() => (Count==0) ? default(T) : this[random.Next(Count)]; }

    /// IterList<T> : List<T>
    /// A simple wrapper for lists which simply steps through the lis
    public class IterList<T> : List<T> {
        int Current = -1; public T Next() => (Count==0 || Current>Count) ? default(T) : this[++Current]; }

    /// LoopList<T> : List<T>
    /// A simple wrapper class for List<T>, which adds the
    /// ability to return a random element from the list.
    public class LoopList<T> : List<T> {
        int Current = -1; public T Next() => (Count==0) ? default(T) : this[++Current%Count]; }


    namespace Astronautics {


        /// FlightMode
        /// defines the different kinds of flight control systems
        public enum FlightMode { Navigation, Assisted, Manual, Manuevering }


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
            public static DateTime Date = new DateTime(2017,1,20); } // apocalypse
    }


    namespace Puzzles {


        /// PuzzleArgs : StoryArgs
        /// encapsulates the most important part of a puzzle: is it solved?
        public class PuzzleArgs<T,U> : StoryArgs {
            public bool IsSolved {get;set;}
            public T Condition {get;set;}
            public U Solution {get;set;}
            public IPiece<T,U> Piece {get;set;}
            public Func<(T condition, U solution)> Solver {get;set;}
        }

        /// PuzzleAction :  event
        /// when a piece is posed, its parent should be notified via this event
        public delegate void PuzzleAction<T,U>(PuzzleArgs<T,U> e=null);
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
            void Wear(StoryArgs e=null);


            /// Stow : () => void
            /// puts away the object
            void Stow(StoryArgs e=null);
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


        public enum StatKind { Health, Endurance, Strength, Agility, Dexterity, Perception, Intellect, Memory }
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


        /// Damage : IDamage
        /// Defines a low-level object to send statistical data
        public struct Damage {
            public float vitality {get;set;}
            public int Critical {get;set;}
            void Hit(int damage) => vitality -= damage;
        }


        /// Stat : object
        /// Base class of a statistic. Can perform Checks and can be used
        /// to process a Hit or some other roll / event based on statistics.
        public class Stat<T> {
            public StatKind kind;
            public bool Check() => true;
            public bool Check(Stat<T> stat) => true;
            protected T value {get;set;}
            public Stat() { }
            public Stat(StatKind kind) { this.kind = kind; }
            public Stat(StatKind kind, T value) : this(kind) { this.value = value; }
            public bool Fits(string s) => kind.ToString()==s;
            public bool Fits(Type type) => kind.ToString()==type.ToString();
        }

        public class StatSet<T> : Stat<T> {
            List<Stat<T>> stats = new List<Stat<T>>();
            public bool IsSynchronized => false;
            public bool IsReadOnly => false;
            public int Count => stats.Count;
            public object SyncRoot => stats;
            public StatSet() { }
            public StatSet(Stat<T>[] stats) { this.stats.AddRange(stats); }
            public StatSet(List<Stat<T>> stats) { this.stats.AddRange(stats); }
            public Stat<T> this[string stat] => stats.First(o => o.Fits(stat));
            public Stat<T> this[Type type] => stats.First(o => o.Fits(type));
            public void CopyTo(Stat<T>[] a, int n) => stats.CopyTo(a, n);
            public IEnumerator GetEnumerator() => stats.GetEnumerator();
        }

        public class HealthStats : StatSet<int> {
            Faculties faculties {get;set;}
            Condition condition {get;set;}
            public void AddCondition(Condition cond) { }
            public void AddConditions(params Condition[] a) => a.ForEach(o => AddCondition(o));
        }
    }


    /// Extensions
    /// a collection of helpful little snippets
    public static partial class Extensions {

        // tuple stuff
        public static void Deconstruct(this Vector3 o,out float x,out float y,out float z) => (x,y,z) = (o.x,o.y,o.z);
        public static void Deconstruct(this (float x,float y,float z) o,out Vector3 v) => v = new Vector3(o.Item1,o.Item2,o.Item3);
        public static void Deconstruct(this Transform o, out Vector3 position, out Quaternion rotation) => (position,rotation) = (o.position,o.rotation);
        public static void AddForce(this Rigidbody o, (float,float,float) force, ForceMode mode=ForceMode.Force) => o.AddForce(force.vect(), mode);
        public static void AddTorque(this Rigidbody o, (float,float,float) force, ForceMode mode=ForceMode.Force) => o.AddTorque(force.vect(), mode);
        public static float Angle(this Vector3 o, Vector3 v) => Vector3.Angle(o,v);
        public static float Angle(this Vector3 o, (float,float,float) v) => o.Angle(v.vect());
        public static float Angle(this (float,float,float) o, Vector3 v) => o.vect().Angle(v);
        public static float Angle(this (float,float,float) o, (float,float,float) v) => o.vect().Angle(v.vect());
        public static float magnitude(this (float,float,float) o) => o.vect().magnitude;
        public static float sqrMagnitude(this (float,float,float) o) => o.vect().sqrMagnitude;
        public static (float x, float y, float z) tuple(this Vector3 o) => (o.x, o.y, o.z);
        public static Vector3 vect(this (float,float,float) o) => new Vector3(o.Item1, o.Item2, o.Item3);
        public static Vector3 normalized(this (float,float,float) o) => o.vect().normalized;

    /// IsFacing : () => bool
    /// detects if the rotation is within a certain angle in degrees
    public static bool IsFacing(this Quaternion o, Quaternion rotation, float angle=0.01f) => Quaternion.Angle(o,rotation)<angle;
    public static bool IsFacing(this Transform o, Quaternion rotation, float angle=0.01f) => Quaternion.Angle(o.rotation,rotation)<angle;
    public static bool IsFacing(this Transform o, Transform rotation, float angle=0.01f) => Quaternion.Angle(o.rotation,rotation.rotation)<angle;

    /// IsNear : () => bool
    /// detects if the transform is close to the location
    public static bool IsNear(this Transform o, Transform location, float distance=0.01f) => o.IsNear(location.position,distance);
    public static bool IsNear(this Transform o, Vector3 position, float distance=0.01f) => Vector3.Distance(o.position,position)<distance;
    public static bool IsNear(this Vector3 o, Vector3 vector, float distance=float.Epsilon) => (o-vector).sqrMagnitude<distance*distance;
    public static bool IsNear(this (float,float,float) o, Vector3 v, float dist=float.Epsilon) => v.IsNear(new Vector3(o.Item1, o.Item2, o.Item3),dist);


        /// Distance : () => real
        /// finds distance between transforms
        public static float Distance(this Transform o, Transform a) => (o.position-a.position).magnitude;


        /// ToColor : (int) => color
        /// convert a number to a color
        public static Color32 ToColor(this int n) => new Color32(
            r: (byte) ((n>>16)&0xFF), g: (byte) ((n>>8)&0xFF), b: (byte) (n&0xFF), a: (byte) (0xFF));


        /// ToColor : (string) => color
        /// convert a string to a color
        public static Color32 ToColor(this string s) => new Color32(
            r: (byte)((System.Convert.ToInt32(s.Substring(1,3),16)>>16)&0xFF),
            g: (byte)((System.Convert.ToInt32(s.Substring(4,6),16)>>8)&0xFF),
            b: (byte)(System.Convert.ToInt32(s.Substring(7,9),16)&0xFF), a: 0xFF);

        /// ToInt : (color) => int
        /// convert a color to a number
        public static int ToInt(this Color32 o) => (byte)((o.r>>16)&0xFF)+(byte)((o.g>>8)&0xFF)+(byte)(o.b&0xFF)+0xFF;

        /// Call : (event) => event()
        /// calls the unity event with the same syntax as literally everything else
        public static void Call(this UnityEvent o) => o.Invoke();
        public static void Call<A>(this UnityEvent<A> o, A a) => o.Invoke(a);
        public static void Call<A,B>(this UnityEvent<A,B> o, A a, B b) => o.Invoke(a,b);
        public static void Call<A,B,C>(this UnityEvent<A,B,C> o, A a, B b, C c) => o.Invoke(a,b,c);
        public static void Call<A,B,C,D>(this UnityEvent<A,B,C,D> o, A a, B b, C c, D d) => o.Invoke(a,b,c,d);

        /// IsNullOrEmpty : (string) => bool
        /// extends the string method for cleaner call syntax
        public static bool IsNullOrEmpty(this string o) => string.IsNullOrEmpty(o);

        /// EscapeURI : (string) => URL
        /// converts a string to a well-formed URI
        public static string EscapeURI(this string o) => System.Uri.EscapeUriString(o);

        /// Capitalize : (string) => String
        /// capitalizes strings
        public static string Capitalize(this string o) => o.FirstOrDefault().ToString().ToUpper()+o.Substring(1);

        /// Ellipsis : (string) => string...
        /// shortens a string to length len, and appends ellipsis
        public static string Ellipsis(this string s, int len=100) => (s.Length<len)?s:s.Substring(0,len-1)+"…";


        /// Hide : (Camera, layer) => void
        /// adds / removes one layer from the culling mask
        public static void Show(this Camera o, string s="Default") => o.cullingMask |= 1<<LayerMask.NameToLayer(s);
        public static void Hide(this Camera o, string s="Default") => o.cullingMask &= ~(1<<LayerMask.NameToLayer(s));


        /// Add : (T[]) => void
        /// I just don't like that AddRange is a different name than Add
        public static void Add<T>(this List<T> o, params T[] a) => o.AddRange(a);
        public static void Add<T>(this List<T> o, IEnumerable<T> a) => o.AddRange(a);


        /// Many : (T[]) => bool
        /// true if the collection has more than one element
        public static bool Many<T>(this IEnumerable<T> list) {
            var enumerator = list.GetEnumerator();
            return enumerator.MoveNext() && enumerator.MoveNext(); }


        /// To : (int) => int[]
        /// creates a range of numbers
        public static IEnumerable<int> To(this int from, int to) {
            if (from < to) while (from <= to) yield return from++;
            else while (from >= to) yield return from--; }


        /// ForEach : (T[]) => void
        /// applies a function to each element of a builtin array
        public static void ForEach<T>(this T[] list, Action<T> func) { foreach (var o in list) func(o); }
        public static void ForEach<T>(this IEnumerable<T> list, Action<T> func) { foreach (var o in list) func(o); }
        public static T Pick<T>(this IList<T> o) => (o.Count==0) ? default(T) : o[new System.Random().Next(o.Count)];

        /// DerivesFrom<T> : (type) => bool
        /// determines if a type is or derives from the type specified
        public static bool DerivesFrom<T>(this Type o) => o==typeof(T) || o.IsSubclassOf(typeof(T));

        /// GetTypes : (type) => type[]
        /// gets a list of all parent types on a given type
        public static IEnumerable<Type> GetTypes(this Type type) {
            if (type == null || type.BaseType == null) yield break;
            foreach (var i in type.GetInterfaces()) yield return i;
            var currentBaseType = type.BaseType;
            while (currentBaseType != null) {
                yield return currentBaseType;
                currentBaseType = currentBaseType.BaseType;
            }
        }

        public static IEnumerable<Type> GetTypes(this Type type, Type root=null) {
            if (type==null || type.BaseType == null) yield break;
            if (root==null) root = typeof(object);
            var current = type;
            while ((current = current.BaseType)!=null)
                if (current==root.BaseType) yield break; else yield return current;
        }

        /// md : (markdown) => html
        /// adds Markdown formatting capability to any string and removes all <p> tags
        public static string md(this string s) => new StringBuilder(Markdown.Transform(s))
            .Replace("<h1>", $"<size={24}><color=#{0x98C8FC:X}>").Replace("</h1>", "</color></size>")
            .Replace("<h2>", $"<size={18}><color=#{0x98C8FC:X}>").Replace("</h2>", "</color></size>")
            .Replace("<h3>", $"<size={16}><color=#{0x98C8FC:X}>").Replace("</h3>","</color></size>")
            .Replace("<h4>", $"<size={14}><color=#{0x98C8FC:X}>").Replace("</h4>","</color></size>")
            .Replace("<pre>","").Replace("</pre>","").Replace("<code>","").Replace("</code>","")
            .Replace("<ul>","").Replace("</ul>","").Replace("<li>","").Replace("</li>","")
            .Replace("<warn>", $"<color=#{0xFA2363:X}>").Replace("</warn>", $"</color>")
            .Replace("<cost>", $"<color=#{0xFFDBBB:X}>").Replace("</cost>", $"</color>")
            .Replace("<item>", $"<color=#{0xFFFFFF:X}>").Replace("</item>", $"</color>")
            .Replace("<cmd>", $"<color=#{0xAAAAAA:X}>").Replace("</cmd>", $"</color>")
            .Replace("<life>", $"<color=#{0x7F1116:X}>").Replace("</life>", $"</color>")
            .Replace("<em>","<i>").Replace("</em>","</i>").Replace("<p>","").Replace("</p>","")
            .Replace("<blockquote>","<i>").Replace("</blockquote>","</i>")
            .Replace("<strong>","<b>").Replace("</strong>","</b>").ToString();

        public static void Disable<T>(this GameObject o) where T : Component => o.Disable<T>();
        public static void Disable<T>(this Component o) where T : Component => o.Disable<T>();
        public static void Disable(this Behaviour o) => o.Enable(false);
        public static void Enable<T>(this GameObject o) where T : Component => o.Enable<T>();
        public static void Enable<T>(this Component o) where T : Component => o.Enable<T>();
        public static void Enable(this Behaviour o, bool isOn=true) => o.enabled = isOn;

        /// Get<T> : (type) => T
        /// gets the attached component or rigorously returns null
        static T GetOrNull<T>(T o) => (o==null)?default(T):o;
        public static T Get<T>(this GameObject o) => GetOrNull<T>(o.GetComponent<T>());
        public static T Get<T>(this Component o) => GetOrNull<T>(o.GetComponent<T>());
        public static T GetParent<T>(this GameObject o) => GetOrNull<T>(o.GetComponentInParent<T>());
        public static T GetParent<T>(this Component o) => GetOrNull<T>(o.GetComponentInParent<T>());
        public static T GetChild<T>(this GameObject o) => GetOrNull<T>(o.GetComponentInChildren<T>());
        public static T GetChild<T>(this Component o) => GetOrNull<T>(o.GetComponentInChildren<T>());
        public static List<T> GetChildren<T>(this GameObject o) => o.GetComponentsInChildren<T>().ToList();
        public static List<T> GetChildren<T>(this Component o) => o.GetComponentsInChildren<T>().ToList();
        public static T Create<T>(GameObject original) where T : Component =>
            Create<T>(original, Vector3.zero, Quaternion.identity);
        public static T Create<T>(GameObject original, Vector3 position) where T : Component =>
            Create<T>(original, position, Quaternion.identity);
        public static T Create<T>(GameObject original, Vector3 position, Quaternion rotation) where T : Component =>
            UnityEngine.Object.Instantiate(original, position, rotation).GetComponent<T>();
    }
}



