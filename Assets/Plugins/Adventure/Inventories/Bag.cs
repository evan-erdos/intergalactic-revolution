/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-24 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Inventories {
    public class Bag : ItemSet {
        string ItemsName => $"It contains:\n{this.Aggregate("",(a,e)=>a+"- "+e)}";
        public override string Content => $"{base.Content}\n{ItemsName}";
        void OnDestroy() => this.ForEach(o => o.Drop());
    }
}
