/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-15 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Inventories {
    public class Crystal : Item, IWieldable {
        public int Count = 24, Size = 8, CountLoaded = 7, BurstCount = 1;
        public float range = 128f, spread = 0.1f, reload = 2f, force = 443f;
        public AudioClip[] firing, impacts, reloads;
        public Transform flashLocation, shellLocation;
        public GameObject fireShell, fireFlash;
        public bool Worn {get;set;}
        public uint Shards {get;set;}
        public Transform Grip => transform;
        public override void Use() => Attack();
        public void Stow() => Log(Description["stow"]);
        public void Wear() => Log(Description["wear"]);
        public void Attack() {
            var instance = Create(fireFlash,flashLocation.position,flashLocation.rotation);
            instance.transform.parent = null;
            var shell = Create(fireShell,shellLocation.position,shellLocation.rotation);
            var rigidbody = shell.GetComponent<Rigidbody>();
            rigidbody.AddRelativeForce(
                force: Vector3.up+Random.insideUnitSphere*0.2f,
                mode: ForceMode.VelocityChange);
            rigidbody.AddRelativeTorque(
                torque: Vector3.up+Random.insideUnitSphere*0.1f,
                mode: ForceMode.VelocityChange);
        }

        static Vector3 Spray(Vector3 direction, float spread) {
            var delta = Random.insideUnitCircle - new Vector2(0.5f,0.5f);
            var splay = new Vector2(direction.x, direction.y) + delta * spread;
            return direction + new Vector3(splay.x, splay.y, 0);
        }
    }
}
