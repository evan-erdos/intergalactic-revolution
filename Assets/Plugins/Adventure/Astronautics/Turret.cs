/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronautics.Spaceships {
    public class Turret : Adventure.Object, IWeapon, IEnumerable<Weapon> {
        bool isFiring;
        new Rigidbody rigidbody;
        List<Weapon> weapons = new List<Weapon>();
        Transform turret;
        [SerializeField] protected Event<AttackArgs> onFire = new Event<AttackArgs>();
        public bool IsDisabled {get;protected set;} = false;
        public float Health {get;protected set;} = 3000;
        public Vector3 Velocity => rigidbody.velocity;
        public Weapon Current {get;protected set;}
        public ITrackable Target {get;set;}
        public event AdventureAction<AttackArgs> FireEvent;
        public IEnumerator<Weapon> GetEnumerator() => weapons.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => weapons.GetEnumerator() as IEnumerator;
        public void Fire(AttackArgs e=null) {
            if (isFiring) StartSemaphore(Firing);
            IEnumerator Firing() {
                if (e is null) e = new AttackArgs {
                    Sender = this, Target = Target, Position = Target.Position,
                    Velocity = Target.Velocity, Displacement = Velocity };
                (Current = weapons[++current%weapons.Count]).Fire(e); FireEvent(e);
                yield return new WaitForSeconds(Current.Rate/weapons.Count);
            }
        } int current = -1; // ick

        public void Disable() { weapons.ForEach(o => o.Disable()); IsDisabled = true; }
        public void Damage(float damage) => If (0>(Health-=damage), () => Kill());
        void Kill() => (rigidbody.isKinematic, transform.parent, IsDisabled) = (false,null,true);

        void Awake() {
            (rigidbody, weapons) = (Get<Rigidbody>(), GetChildren<Weapon>());
            Current = weapons.First();
            FireEvent += e => onFire?.Call(e);
            foreach (Transform o in transform) if (o.name=="turret") turret = o;
        }

        IEnumerator Start() {
            StartCoroutine(WaitFire());
            while (true) {
                yield return new WaitForSeconds(1);
                if (Target is null) continue;
                var displacement = Target.Position-turret.position;
                var rotation = Quaternion.LookRotation(displacement, transform.up);
                while (turret.rotation!=rotation) {
                    yield return new WaitForFixedUpdate();
                    displacement = Target.Position-turret.position;
                    turret.rotation = Quaternion.Slerp(turret.rotation,
                        Quaternion.LookRotation(displacement,transform.up), 2*Time.fixedDeltaTime);
                }
            }

            IEnumerator WaitFire() { while (!IsDisabled) {
                yield return new WaitForSeconds(4); isFiring = !isFiring; } }
        }
    }
}
