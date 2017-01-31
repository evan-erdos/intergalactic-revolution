/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronautics.Spaceships {
    public class HyperspaceBarrier : SpaceObject {
        void OnTriggerExit(Collider o) =>
            If(IsValid(o), () => Jump(o.GetParent<Spaceship>()));
        bool IsValid(Collider o) => !(o.GetParent<Spaceship>() is null);
        void Jump(Spaceship o) => o.HyperJump(o.transform.rotation);
    }
}
