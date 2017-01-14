/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Cameras;

namespace Adventure.Astronautics.Spaceships {
    public class SpacePlayer : NetworkSpaceObject {
        (float x,float y) mouse = (0,0);
        List<NetworkStartPosition> points = new List<NetworkStartPosition>();
        [SerializeField] protected Spaceship spaceship;
        [SerializeField] List<GameObject> randomShips = new List<GameObject>();

        public void SetSpaceship(Spaceship spaceship) {
            if (spaceship is null) return;
            spaceship.Create();
            spaceship.GetComponentsInChildren<ISpaceObject>().ForEach(o=>o.Create());
            this.spaceship = spaceship;
            GetOrAdd<SpaceshipController>().Ship = spaceship;
            spaceship.KillEvent += (o,e) => OnKill();
            spaceship.JumpEvent += (o,e) => OnJump();
            transform.parent = spaceship.transform;
            SetCamera();
            spaceship.ToggleView();
        }

        void Awake() => points.AddRange(FindObjectsOfType<NetworkStartPosition>());
        void Start() => SetSpaceship(Create<Spaceship>(randomShips.Pick()));
        void OnNetworkInstantiate(NetworkMessageInfo info) => SetCamera();

        void Update() => mouse = (Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));
        void FixedUpdate() => transform.localRotation = Quaternion.Euler(
            x: Mathf.Clamp(transform.localEulerAngles.x+mouse.y*10,-60,60),
            y: transform.localEulerAngles.y+mouse.x*10, z: 0);

        void SetCamera() {
            if (GetComponent<NetworkIdentity>().localPlayerAuthority)
                PlayerCamera.Follow(spaceship.transform); }

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
                // SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
                var point = points.Pick();
                transform.parent = null;
                transform.position = point.transform.position;
                transform.rotation = point.transform.rotation;
                Start();
                yield return new WaitForSeconds(5);
            }
        }
    }
}
