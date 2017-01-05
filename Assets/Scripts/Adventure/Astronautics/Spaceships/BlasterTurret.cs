/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace Adventure.Astronautics.Spaceships {
    public class BlasterTurret : SpaceObject, IWeapon, IEnumerable<Blaster> {
        bool isDisabled, isFiring;
        int current;
        new Rigidbody rigidbody;
        [SerializeField] float delay = 0.1f;
        [SerializeField] float force = 4f;
        [SerializeField] float speed = 2;
        [SerializeField] protected Transform trackingTarget;
        List<Blaster> list = new List<Blaster>();
        Transform turret;

        public float Health {get;protected set;} = 3000;
        public Blaster Current {get;protected set;}
        public Transform Target => trackingTarget;
        public IEnumerator<Blaster> GetEnumerator() => list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator() as IEnumerator;
        public void Fire() => Fire(transform.forward);
        public void Fire(Vector3 position) => Fire(position,rigidbody.velocity);
        public void Fire(Vector3 position, Vector3 velocity) =>
            Fire(position, Quaternion.LookRotation(transform.forward), velocity);
        public void Fire(Vector3 position, Quaternion rotation, Vector3 velocity) =>
            Fire(position.ToTuple(), rotation, velocity.ToTuple(), delay, force);
        public void Fire(
                        (float,float,float) position,
                        Quaternion rotation,
                        (float,float,float) velocity) =>
            Fire(position, rotation, velocity, delay, force);

        protected void Fire(
                        (float,float,float) position,
                        Quaternion rotation,
                        (float,float,float) velocity,
                        float delay=0.2f,
                        float force=4f) {
            if (isFiring) StartSemaphore(Firing);
            IEnumerator Firing() {
                Current = list[++current%list.Count];
                Current.Fire(position,rotation,velocity);
                yield return new WaitForSeconds(delay/list.Count);
            }
        }

        public void Disable() {
            isDisabled = true;
            list.ForEach(blaster => blaster.Disable());
        }

        public void Damage(float damage) {
            Health -= damage;
            if (0<Health) return;
            rigidbody.isKinematic = false;
            transform.parent = null;
            isDisabled = true;
        }

        void Awake() {
            rigidbody = Get<Rigidbody>();
            list.AddRange(GetComponentsInChildren<Blaster>());
            Current = list.First();
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

            IEnumerator WaitFire() {
                while (!isDisabled) {
                    yield return new WaitForSeconds(4);
                    isFiring = !isFiring;
                }
            }
        }
    }
}
