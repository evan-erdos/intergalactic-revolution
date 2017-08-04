/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-10 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Adventure {


    /// ISocket
    /// an object socket in which objects can be placed
    public interface ISocket<T> : IObject where T : IObject {


        /// Target : T
        /// object currently in the socket
        T Target {get;set;}


        /// Clear : () => void
        /// unparents the target and sets it loose
        void Clear();


        /// SetTarget : (T) => void
        /// puts the target into the socket
        void SetTarget(T target);
    }
}
