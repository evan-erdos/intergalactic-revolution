/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;
using unit=Adventure.Astronomy.Units;

namespace Adventure.Astronomy.Aeronautics {
    public class Planet : SpaceObject {
        [SerializeField] float period = 24 * unit.Day;
        [SerializeField] float orbit = 365 * unit.Day;
        [SerializeField] float distance = 400 * unit.km;
        Transform primary, planet;

        IEnumerator Start() {
            planet = transform.Find("planet");
            primary = transform.parent;
            transform.localPosition = Vector3.forward*distance;
            transform.RotateAround(
                primary.position,
                Vector3.up,
                unit.Seed%360);
            planet.Rotate(0, unit.Seed%360, 0);
            while (true) {
                yield return new WaitForSeconds(1);
                yield return new WaitForFixedUpdate();
                transform.RotateAround(
                    primary.position,
                    Vector3.up,
                    unit.Time/orbit);
                planet.Rotate(0, unit.Time/period, 0);
            }
        }
    }
}
