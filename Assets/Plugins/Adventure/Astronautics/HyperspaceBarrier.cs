/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronautics.Spaceships {
    public class HyperspaceBarrier : Adventure.Object {
        async void Start() { await 1; while (true) { await 1;
            if (!Manager.ship.Position.IsNear(transform.position,10000))
                Manager.ship.HyperJump(Manager.ship.transform.rotation); } }
        // void OnTriggerExit(Collider c) => If(c.GetParent<Spaceship>(), o => o.HyperJump(o.transform.rotation));
    }
}
