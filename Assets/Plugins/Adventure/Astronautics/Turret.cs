/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Adventure.Astronautics.Spaceships {
    public class Turret : Adventure.Object, IWeapon, IEnumerable<IWeapon> {
        bool isFiring;
        new Rigidbody rigidbody;
        List<IWeapon> weapons = new List<IWeapon>();
        Transform turret;
        [SerializeField] protected CombatEvent onFire = new CombatEvent();
        public event AdventureAction<CombatArgs> FireEvent;
        public bool IsDisabled {get;protected set;} = false;
        public float Health {get;protected set;} = 3000;
        public float Rate => CurrentWeapon.Rate;
        public Vector3 Velocity => rigidbody.velocity;
        public IWeapon CurrentWeapon {get;protected set;}
        public ITrackable Target {get;set;}
        public IEnumerator<IWeapon> GetEnumerator() => weapons.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => weapons.GetEnumerator() as IEnumerator;
        public void Fire(CombatArgs e=null) {
            bool PreFire() => isFiring;
            if (PreFire()) FireEvent(e ?? new CombatArgs { Sender=this, Target=Target,
                Position=Target.Position, Velocity=Target.Velocity, Displacement=Velocity });
        }

        async Task OnFire(CombatArgs e) { foreach (var o in Weapons()) { o.Fire(e); await (o.Rate/weapons.Count); } }
        IEnumerable<IWeapon> Weapons() { var i=0; while (true) { yield return weapons[i++%weapons.Count]; } }

        public void Disable() { weapons.ForEach(o => o.Disable()); IsDisabled = true; }
        public void Damage(float damage) { if (0>(Health-=damage)) Kill(); }
        void Kill() => (rigidbody.isKinematic, transform.parent, IsDisabled) = (false,null,true);

        void Awake() {
            (rigidbody, weapons) = (Get<Rigidbody>(), GetChildren<IWeapon>());
            CurrentWeapon = weapons.First();
            FireEvent += e => onFire?.Call(e); onFire.Add(e => StartAsync(() => OnFire(e)));
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
                    rotation = Quaternion.LookRotation(displacement,transform.up);
                    turret.rotation = Quaternion.Slerp(turret.rotation, rotation, 2*Time.fixedDeltaTime);
                }
            }

            IEnumerator WaitFire() { while (!IsDisabled) {
                yield return new WaitForSeconds(4); isFiring = !isFiring; } }
        }
    }
}
