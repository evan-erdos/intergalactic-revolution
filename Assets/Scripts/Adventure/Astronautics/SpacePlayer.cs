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
        [SerializeField] List<GameObject> ships = new List<GameObject>();

        public void SetShip() => CmdCreateShip(ships.Pick());
        [Command] public void CmdCreateShip(GameObject prefab) {
            var instance = Instantiate(prefab) as GameObject;
            NetworkServer.Spawn(instance);
            var spaceship = instance.Get<Spaceship>();
            spaceship.Create();
            spaceship.GetComponentsInChildren<ISpaceObject>().ForEach(o=>o.Create());
            this.spaceship = spaceship;
            GetOrAdd<SpaceshipController>().Ship = spaceship;
            spaceship.KillEvent += (o,e) => OnKill();
            spaceship.JumpEvent += (o,e) => OnJump();
            // transform.parent = spaceship.transform;
            SetCamera();
            spaceship.ToggleView();
        }

        void Awake() => points.AddRange(FindObjectsOfType<NetworkStartPosition>());
        // bool IsMainPlayer() => Get<NetworkIdentity>().localPlayerAuthority;
        bool IsMainPlayer() => isLocalPlayer;
        void Start() => If(IsMainPlayer,() => SetShip());
        public override void OnStartLocalPlayer() {
            base.OnStartLocalPlayer(); SetShip(); }

        void OnNetworkInstantiate(NetworkMessageInfo info) => SetCamera();

        void Update() => mouse = (Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));
        void FixedUpdate() {
            transform.localRotation = Quaternion.Euler(
                x: Mathf.Clamp(transform.localEulerAngles.x+mouse.y*10,-60,60),
                y: transform.localEulerAngles.y+mouse.x*10, z: 0);
            if (spaceship is null) return;
            transform.position = spaceship.transform.position;
            transform.rotation = spaceship.transform.rotation;
        }

        void SetCamera() => If(IsMainPlayer, () =>
            PlayerCamera.Follow(spaceship.transform));

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

