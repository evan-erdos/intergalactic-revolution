/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-10 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Adventure {


    /// IObject
    /// shared interface for all of the things
    public interface IObject {


        /// Name : string
        /// an identifying string for this object
        string Name {get;}


        /// Radius : real
        /// determines event ranges in meters (sqrMagnitude)
        float Radius {get;}


        /// Position : (real,real,real)
        /// represents the object's world position
        Vector3 Position {get;} // (float x, float y, float z)


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


        /// If : (() => bool, () => void) => void
        /// functional-style if statement
        bool If(bool cond, Action then);
        bool If(Func<bool> cond, Action then);
        bool If<T>(T cond, Action<T> then);


        /// Fits : (string) => bool
        /// matches the pattern for this object
        bool Fits(string pattern);


        /// StartSemaphore : (coroutine) => void
        /// like StartCoroutine, but ignores calls until
        void StartSemaphore(Func<IEnumerator> func);

        /// StartAsync : (thread) => void
        /// like StartSemaphore, but ignores subsequent async calls
        void StartAsync(Func<Task> func);


        /// Get : <T>() => T
        /// gets the component T or null
        T Get<T>();
        T GetParent<T>();
        T GetChild<T>();
        List<T> GetChildren<T>();


        /// Find : <T>() => Thing[]
        /// a collections of all nearby things within a range
        List<T> Find<T>(float radius=5) where T : IThing;
    }
}
