/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;
using unit=Adventure.Astronautics.Astronomy;

namespace Adventure.Astronautics {
    public class Sun : SpaceObject {
        [SerializeField] float period = 24; // days
        IEnumerator Start() {
            var sun = transform.Find("sun");
            sun.Rotate(0,unit.Time%360,0);
            while (true) {
                yield return new WaitForSeconds(1);
                yield return new WaitForFixedUpdate();
                sun.Rotate(0,(unit.Time*unit.Day)/period,0);
            }
        }
    }
}
