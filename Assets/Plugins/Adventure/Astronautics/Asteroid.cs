/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-01 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronautics {
    public class Asteroid : Adventure.Object {
        async void Start() {
            var rotation = Random.insideUnitSphere*Time.fixedDeltaTime;
            await Random.value;
            while (true) { await 0.1; transform.Rotate(rotation); }
        }
    }
}
