/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-15 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Inventories {
    public class Crystal : Item, IWieldable {
        public int Count = 24, Size = 8, CountLoaded = 7, BurstCount = 1;
        public float range = 128, spread = 0.1f, reload = 2, force = 443;
        public AudioClip[] firing, impacts, reloads;
        public Transform flashLocation, shellLocation;
        public GameObject fireShell, fireFlash;
        public bool Worn {get;set;}
        public uint Shards {get;set;}
        public Transform Grip => transform;
        public override void Use() => Attack();
        public void Stow(StoryArgs e=null) => Log(Description["stow"]);
        public void Wear(StoryArgs e=null) => Log(Description["wear"]);
        public void Attack() {
            var instance = Create(fireFlash, flashLocation.position, flashLocation.rotation);
            instance.transform.parent = null;
            var shell = Create(fireShell, shellLocation.position, shellLocation.rotation);
            var rb = shell.Get<Rigidbody>();
            rb.AddRelativeForce(Vector3.up+Random.insideUnitSphere*0.2f,ForceMode.VelocityChange);
            rb.AddRelativeTorque(Vector3.up+Random.insideUnitSphere*0.1f, ForceMode.VelocityChange);
            // Vector3 Spray(Vector3 direction, float spread) {
            //     var delta = Random.insideUnitCircle - new Vector2(0.5f,0.5f);
            //     var splay = new Vector2(direction.x, direction.y) + delta * spread;
            //     return direction + new Vector3(splay.x, splay.y, 0);
            // }
        }
    }
}
