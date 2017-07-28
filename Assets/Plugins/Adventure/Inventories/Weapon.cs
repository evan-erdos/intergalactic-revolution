/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-07-07 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Inventories {
    public class Weapon : Item, IWieldable {
        public AudioClip[] attackSounds;
        public bool Worn {get;set;}
        public uint Uses {get;set;}
        public float Rate {get;set;}
        public Transform Grip => transform;
        public override void Do() => Attack();
        public override void Use() => Attack();
        public virtual void Attack() => Log($"You attack with the {name}");
        public void Stow(StoryArgs e=null) { }
        public void Wear(StoryArgs e=null) { }
        new public class Data : Item.Data {
            public float rate {get;set;}
            public override Object Deserialize(Object o) {
                var instance = base.Deserialize(o) as Weapon;
                instance.Rate = this.rate;
                return instance;
            }
        }
    }
}
