/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;
using unit=Adventure.Astronautics.Astronomy;

namespace Adventure.Astronautics {
    public class Planet : SpaceObject {
        [SerializeField] float period = 24; // days
        [SerializeField] float orbit = 365; // days
        [SerializeField] float distance = 400; // km
        Transform primary, planet;

        IEnumerator Start() {
            planet = transform.Find("planet");
            primary = transform.parent;
            transform.localPosition = Vector3.forward * (distance*unit.km);
            transform.RotateAround(primary.position,Vector3.up,unit.Time%360);
            planet.Rotate(0,unit.Time%360,0);
            while (true) {
                yield return new WaitForSeconds(1);
                yield return new WaitForFixedUpdate();
                transform.RotateAround(
                    primary.position,Vector3.up,unit.Time/(orbit*unit.Day));
                planet.Rotate(0,unit.Time/(period*unit.Day),0);
            }
        }
    }
}
