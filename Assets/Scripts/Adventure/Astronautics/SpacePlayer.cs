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
        new PlayerCamera camera;
        List<NetworkStartPosition> points = new List<NetworkStartPosition>();
        [SerializeField] List<GameObject> ships = new List<GameObject>();
        public Spaceship Ship {get;protected set;}

        public void CreateShip() => CmdCreateShip(ships.Pick());
        [Command] public void CmdCreateShip(GameObject prefab) {
            var instance = Instantiate(prefab) as GameObject;
            NetworkServer.Spawn(instance);
            Ship = instance.Get<Spaceship>();
            Ship.Create();
            Ship.GetComponentsInChildren<ISpaceObject>().ForEach(o=>o.Create());
            GetOrAdd<SpaceshipController>().Ship = Ship;
            Ship.KillEvent += (o,e) => OnKill();
            Ship.JumpEvent += (o,e) => OnJump();
        }

        void Awake() => points.AddRange(FindObjectsOfType<NetworkStartPosition>());
        void Start() => camera = GetComponentInChildren<PlayerCamera>();
        public override void OnStartLocalPlayer() {
            CreateShip();
            if (isLocalPlayer) PlayerCamera.Follow(Ship.transform);
            else camera.GetComponent<Camera>().enabled = false;
        }
        // void OnConnectedToServer() => CreateShip();
        // void OnNetworkInstantiate(NetworkMessageInfo info) => CreateShip();
        void Update() => mouse = (Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));
        void FixedUpdate() {
            transform.localRotation = Quaternion.Euler(
                x: Mathf.Clamp(transform.localEulerAngles.x+mouse.y*10,-60,60),
                y: transform.localEulerAngles.y+mouse.x*10, z: 0);
            if (Ship is null) return;
            transform.position = Ship.transform.position;
            transform.rotation = Ship.transform.rotation;
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
                // SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
                var point = points.Pick();
                transform.parent = null;
                transform.position = point.transform.position;
                transform.rotation = point.transform.rotation;
                CreateShip();
                yield return new WaitForSeconds(5);
            }
        }
    }
}

