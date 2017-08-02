/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;
using Adventure.Astronautics.Spaceships;

namespace Adventure {
    public class Boundary : Adventure.Object {
        [SerializeField] protected ObjectEvent onExit = new ObjectEvent();
        public event AdventureAction<ObjectArgs> ExitEvent;
        public void Exit(ObjectArgs e=null) => ExitEvent(e ?? new ObjectArgs { Sender=this });
        public void OnExit(ObjectArgs e) { if (e.Target is Spaceship ship) ship.HyperJump(); }
        public bool IsInBounds(IObject o, float range=10000) => o?.IsNear(this,range)==true;

        IEnumerator Start() {
            ExitEvent += e => onExit?.Call(e);
            var user = Manager.ship;
            while (user is Spaceship ship) {
                yield return new WaitForSeconds(1);
                if (IsInBounds(user)) continue;
                Exit(new ObjectArgs { Sender=this, Target=user, Position=user.Position });
                yield break;
            }
        }
    }
}
