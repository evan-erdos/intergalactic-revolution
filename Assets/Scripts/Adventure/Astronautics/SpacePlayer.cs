/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Cameras;

namespace Adventure.Astronautics.Spaceships {
    public class SpacePlayer : SpaceObject {
        float mouseX, mouseY;
        [SerializeField] protected GameObject spaceCamera;
        [SerializeField] protected Spaceship spaceship;
        [SerializeField] protected GameObject escapePod;
        [SerializeField] protected GameObject miniHUD;
        [SerializeField] protected GameObject fullHUD;

        void Awake() {
            if (escapePod) Get<Cam3D>()?.SetTarget(escapePod.transform);
            if (!spaceship) throw new SpaceException("No Spaceship!");
            var controller = Get<SpaceshipController>();
            controller.Ship = spaceship;
            controller.miniHUD = miniHUD;
            controller.fullHUD = fullHUD;
            fullHUD.SetActive(false);
            spaceship.KillEvent += (o,e) => OnKill();
            spaceship.JumpEvent += (o,e) => OnJump();
        }

        void Start() {
            transform.parent = spaceship.transform;
            if (!spaceCamera) return;
            var camera = Camera.main;
            var instance = Create<Camera>(spaceCamera);
            instance.rect = camera.rect;
            instance.transform.localPosition = Vector3.zero;
        }

        void Update() =>
            (mouseX,mouseY) = (Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));

        void FixedUpdate() =>
            transform.localRotation = Quaternion.Euler(
                x: transform.localRotation.eulerAngles.x+mouseY*10,
                y: transform.localRotation.eulerAngles.y+mouseX*10,
                z: 0);



        void OnKill() { transform.parent = escapePod.transform; Restart(); }

        void OnJump() {
            StartSemaphore(Jumping);
            IEnumerator Jumping() {
                yield return new WaitForSeconds(1);
                SceneManager.LoadSceneAsync("Moon Base Delta");
            }
        }

        public void Restart() {
            StartSemaphore(Restarting);
            IEnumerator Restarting() {
                yield return new WaitForSeconds(5);
                SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
                yield return new WaitForSeconds(5);
            }
        }
    }
}
