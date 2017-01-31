/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Adventure.Astronautics.Spaceships {
    public class SpacePlayer : SpaceActor {
        (float x,float y) mouse = (0,0);
        SpaceshipController control;

        public override void SetShip(Spaceship ship) {
            Ship = ship;
            Ship.KillEvent += (o,e) => OnKill();
            Ship.JumpEvent += (o,e) => OnJump();
            PlayerCamera.Target = Ship.transform;
        }

        // public override void OnStartLocalPlayer() => CreateShip();
        protected override void Awake() { base.Awake();
            control = GetOrAdd<SpaceshipController>(); }

        // void OnNetworkInstantiate(NetworkMessageInfo info) => CreateShip();
        void Update() => mouse = (Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));
        protected override void FixedUpdate() {
            transform.localRotation = Quaternion.Euler(
                x: Mathf.Clamp(transform.localEulerAngles.x+mouse.y*10,-60,60),
                y: transform.localEulerAngles.y+mouse.x*10, z: 0);
            base.FixedUpdate();
        }

        void OnJump() {
            StartSemaphore(Jumping);
            IEnumerator Jumping() {
                yield return new WaitForSeconds(1);
                SceneManager.LoadSceneAsync("Moon Base Delta");
            }
        }

        public void OnKill() {
            StartSemaphore(Killing);
            IEnumerator Killing() {
                yield return new WaitForSeconds(8);
                PlayerCamera.Target = null;
                SpaceManager.LoadSceneFade("Menu", Color.black);
                yield return new WaitForSeconds(5);
            }
        }
    }
}

