/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronautics.Spaceships {
    public class HyperspaceBarrier : Adventure.Object {
        IEnumerator Start() { while (true) { yield return new WaitForSeconds(1);
            if (true==!Manager.ship?.Position.IsNear(transform.position,10000)) {
                Manager.ship.HyperJump(Manager.ship.transform.rotation); yield break; } } }
    }
}
