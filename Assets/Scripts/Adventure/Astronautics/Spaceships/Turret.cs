﻿/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace Adventure.Astronautics.Spaceships {
    public class Turret : SpaceObject, IWeapon, IEnumerable<Blaster> {
        bool isFiring;
        new Rigidbody rigidbody;
        List<Blaster> list = new List<Blaster>();
        Transform turret;
        public bool IsDisabled {get;protected set;} = false;
        public float Health {get;protected set;} = 3000;
        public Blaster Current {get;protected set;}
        public ITrackable Target {get;set;}
        public IEnumerator<Blaster> GetEnumerator() => list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator() as IEnumerator;
        public void Fire() => Fire(Target);
        public void Fire(ITrackable target) => Fire(target.Position, target.Velocity);
        public void Fire(Vector3 position) => Fire(position,rigidbody.velocity);
        public void Fire(Vector3 position, Vector3 velocity) =>
            Fire(position.tuple(), velocity.tuple());

        int current = -1;
        public void Fire(
                        (float,float,float) position,
                        (float,float,float) velocity) {
            if (isFiring) StartSemaphore(Firing);
            IEnumerator Firing() {
                Current = list[++current%list.Count];
                Current.Fire(position,velocity);
                yield return new WaitForSeconds(Current.Rate/list.Count);
            }
        }

        public void Disable() {
            IsDisabled = true;
            list.ForEach(blaster => blaster.Disable());
        }

        public void Damage(float damage) {
            Health -= damage;
            if (0<Health) return;
            rigidbody.isKinematic = false;
            transform.parent = null;
            IsDisabled = true;
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
            while (true) {
                yield return new WaitForSeconds(1);
                if (Target is null) continue;
                var rotation = Quaternion.LookRotation(
                    Target.Position.vect()-turret.position,
                    transform.up);
                while (turret.rotation!=rotation) {
                    yield return new WaitForFixedUpdate();
                    turret.rotation = Quaternion.Slerp(
                        turret.rotation,
                        Quaternion.LookRotation(
                            Target.Position.vect()-turret.position,
                            transform.up),
                        2*Time.fixedDeltaTime);
                }
            }

            IEnumerator WaitFire() {
                while (!IsDisabled) {
                    yield return new WaitForSeconds(4);
                    isFiring = !isFiring;
                }
            }
        }
    }
}
