/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-01 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronautics {
    public class Asteroid : SpaceObject {
        IEnumerator Start() {
            var rotation = Random.insideUnitSphere*Time.fixedDeltaTime;
            yield return new WaitForSeconds(Random.value);
            while (true) {
                yield return new WaitForSeconds(0.1f);
                transform.Rotate(rotation);
            }
        }
    }
}
