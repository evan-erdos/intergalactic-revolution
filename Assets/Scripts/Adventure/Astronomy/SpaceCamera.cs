/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;

public class SpaceCamera : MonoBehaviour {
    public GameObject solarSystem;
    IEnumerator Start() {
        if (solarSystem) {
            transform.parent = solarSystem.transform;
            transform.localPosition = Vector3.zero;
        } while (true) {
            yield return new WaitForFixedUpdate();
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}
