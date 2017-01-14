/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-01 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronautics {
    public class Asteroid : SpaceObject {
        IEnumerator Start() {
            var rotation = Random.insideUnitSphere*Time.fixedDeltaTime;
            var wait = new WaitForSeconds(0.1f);
            yield return new WaitForSeconds(Random.value);
            while (true) { yield return wait; transform.Rotate(rotation); }
        }
    }
}
