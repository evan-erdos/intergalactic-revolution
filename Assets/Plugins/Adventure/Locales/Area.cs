/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2014-07-06 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Locales {
    public class Area : Thing {

        /// IsSafe : bool
        /// can this Area can be traversed by default?
        public bool IsSafe {get; protected set;}

        /// rooms : Room[]
        /// a set of rooms contained by the Area
        public List<Room> Rooms {get;} = new List<Room>();

        /// areas : Area[]
        /// a set of adjacent Areas
        public List<Area> Areas {get;} = new List<Area>();

        new public class Data : Thing.Data {
            public bool safe {get;set;}
            public List<Room.Data> Rooms {get;set;}
            public override BaseObject Deserialize(BaseObject o) {
                var area = base.Deserialize(o) as Area;
                area.IsSafe = this.safe;
                return area;
            }
        }
    }
}
