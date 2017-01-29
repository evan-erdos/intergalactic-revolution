/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2014-07-06 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Inventories {

    /// Key : Item
    /// any object which can be used to unlock something
    public class Key : Item {

        /// Kind : Keys
        /// Denotes a general category of the Key's shape and
        /// function as an abstraction of the number of breakers
        /// or the warding or whatever.
        public Keys Kind {get;set;}

        /// Value : int
        /// A value to use in the case that an ILockable lacks
        /// a Key object, then it simply compares the numbers
        /// to deterimine if the key fits.
        public int Value {get;set;}
    }
}
