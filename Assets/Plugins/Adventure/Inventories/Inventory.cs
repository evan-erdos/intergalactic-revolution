/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-22 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Inventories {
    public class Inventory<T> : Thing, IEnumerable<T> where T : IItem {

        protected override void Awake() { base.Awake();
            gameObject.layer = LayerMask.NameToLayer("Item");
            rigidbody = GetOrAdd<Rigidbody>();
            onTake.AddListener((o,e) => OnTake());
            onDrop.AddListener((o,e) => OnDrop());
            TakeEvent += (o,e) => onTake?.Invoke(o,e);
            DropEvent += (o,e) => onDrop?.Invoke(o,e);
        }

        new public class Data : Thing.Data {
            public decimal? cost {get;set;}
            public float mass {get;set;}
            public override Object Deserialize(Object o) {
                var instance = base.Deserialize(o) as Item;
                instance.Cost = this.cost;
                instance.Mass = this.mass;
                return instance;
            }
        }
    }
}
