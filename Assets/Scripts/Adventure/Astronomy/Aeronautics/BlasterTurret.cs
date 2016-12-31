/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace Adventure.Astronomy.Aeronautics {
    public class BlasterTurret : Blaster { //Enumerable<Blaster>
        bool waitFire, isFiring;
        int current;
        [SerializeField] float speed = 2;
        [SerializeField] protected Transform trackingTarget;
        List<Blaster> blasters = new List<Blaster>();
        Transform turret;
        public Blaster Current {get;protected set;}
        public Transform Target => trackingTarget;

        protected override void Awake() { base.Awake();
            blasters.AddRange(GetComponentsInChildren<Blaster>());
            if (blasters.Contains(this)) blasters.Remove(this);
            Current = blasters.First();
            foreach (Transform child in transform)
                if (child.name=="turret") turret = child;
        }

        IEnumerator Start() {
            StartCoroutine(WaitFire());
            while (true) { // turret.LookAt(Target);
                yield return new WaitForSeconds(1);
                var rotation = Quaternion.LookRotation(
                    Target.position-turret.position);
                while (turret.rotation != rotation) {
                    yield return new WaitForFixedUpdate();
                    turret.rotation = Quaternion.Slerp(
                        turret.rotation,
                        Quaternion.LookRotation(
                            Target.position-turret.position),
                        speed*Time.fixedDeltaTime);
                }
            }
        }

        IEnumerator WaitFire() {
            var instruction = new WaitForSeconds(4);
            while (true) { yield return instruction; isFiring = !isFiring; }
        }


        public override void Disable() { base.Disable();
            blasters.ForEach(blaster => blaster.Disable()); }

        protected override void Fire(
                        GameObject prefab,
                        Vector3 position,
                        Quaternion rotation,
                        Vector3 velocity,
                        float delay=0.2f,
                        float force=4f) {
            if (!waitFire && isFiring) StartCoroutine(Firing(
                original: prefab,
                position: position,
                rotation: rotation,
                velocity: velocity,
                delay: delay,
                force: force)); }

        IEnumerator Firing(
                        GameObject original,
                        Vector3 position,
                        Quaternion rotation,
                        Vector3 velocity,
                        float delay=0.2f,
                        float force=4f) {
            waitFire = true;
            Current = blasters[++current%blasters.Count];
            Current.Fire(position,velocity,rotation);
            yield return new WaitForSeconds(delay/blasters.Count);
            waitFire = false;
        }
    }
}
