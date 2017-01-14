/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Cameras;

namespace Adventure.Astronautics.Spaceships {
    public class SpacePlayer : NetworkSpaceObject {
        float perlin;
        (float x,float y) mouse = (0,0);
        [SerializeField] protected Spaceship spaceship;
        [SerializeField] List<GameObject> randomShips = new List<GameObject>();

        public void SetSpaceship(Spaceship spaceship) {
            if (spaceship is null) return;
            spaceship.Create();
            spaceship.GetComponentsInChildren<ISpaceObject>().ForEach(o => o.Create());
            this.spaceship = spaceship;
            GetOrAdd<SpaceshipController>().Ship = spaceship;
            spaceship.KillEvent += (o,e) => OnKill();
            spaceship.JumpEvent += (o,e) => OnJump();
            transform.parent = spaceship.transform;
        }

        void Awake() => SetSpaceship(Create<Spaceship>(randomShips.Pick()));
        void Start() => perlin = Random.Range(0,100);
        void Update() => mouse = (Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));
        void OnNetworkInstantiate(NetworkMessageInfo info) => SetCamera();

        void SetCamera() {
            if (!GetComponent<NetworkIdentity>().localPlayerAuthority) return;
            Camera.main.transform.parent = transform;
            Camera.main.transform.localPosition = Vector3.zero;
        }

        void FixedUpdate() {
            var (amount, range) = (0.15f,60);
            // if (spaceship) Camera.main.transform.localPosition =
            //     Random.insideUnitCircle * amount *
            //     (Mathf.PerlinNoise(Time.time*amount,perlin)-0.5f) *
            //     Mathf.Clamp(spaceship.Speed/spaceship.TopSpeed-1,0,0.5f);
            transform.localRotation = Quaternion.Euler(
                x: Mathf.Clamp(transform.localEulerAngles.x+mouse.y*10,-range,range),
                y: transform.localEulerAngles.y+mouse.x*10, z: 0);
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
                SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
                yield return new WaitForSeconds(5);
            }
        }
    }
}
