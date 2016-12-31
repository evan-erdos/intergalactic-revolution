/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;
using unit=Adventure.Astronomy.Units;

namespace Adventure.Astronomy.Aeronautics {
    public class Sun : SpaceObject {
        Transform sun;
        [SerializeField] float period = 24 * unit.Day;
        IEnumerator Start() {
            sun = transform.Find("sun");
            sun.Rotate(0, unit.Seed%360, 0);
            while (true) {
                yield return new WaitForSeconds(1);
                yield return new WaitForFixedUpdate();
                sun.Rotate(0, unit.Time/period, 0);
            }
        }
    }
}
