/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Adventure.Astronomy.Aeronautics {
    public class SpacePlayer : SpaceObject {
        bool once;
        [SerializeField] protected GameObject spaceCamera;
        [SerializeField] protected GameObject spaceship;
        [SerializeField] protected GameObject solarSystem;

        void Start() {
            // SceneManager.LoadSceneAsync(
            //     sceneName: "Base",
            //     mode: LoadSceneMode.Additive);
            // SceneManager.LoadSceneAsync(
            //     sceneName: "Epsilon Eridani",
            //     mode: LoadSceneMode.Additive);
            transform.parent = spaceship.transform;
            if (!spaceCamera) return;
            var instance = Instantiate(spaceCamera) as GameObject;
            var camera = Camera.main;
            var spacecam = instance.GetComponent<Camera>();
            spacecam.rect = camera.rect;
            if (!solarSystem) return;
            spacecam.transform.parent = solarSystem.transform;
            spacecam.transform.localPosition = Vector3.zero;
        }

        public void Restart() {
            if (once) return; once = true;
            StartCoroutine(Restarting());
        }

        IEnumerator Restarting() {
            yield return new WaitForSeconds(5f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
