/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Adventure.Astronautics.Spaceships {
    public class SpacePlayer : SpaceActor {
        (float x, float y) mouse = (0,0);
        SpaceshipController control;

        public override void SetShip(Spaceship ship) {
            Ship = ship;
            Ship.KillEvent += (o,e) => OnKill();
            Ship.JumpEvent += (o,e) => OnJump();
            PlayerCamera.Target = Ship.transform;
        }

        // public override void OnStartLocalPlayer() => CreateShip();
        protected override void Awake() { base.Awake(); control = GetOrAdd<SpaceshipController>(); }

        // void OnNetworkInstantiate(NetworkMessageInfo info) => CreateShip();
        void Update() => mouse = (Input.GetAxis("Lateral"), Input.GetAxis("Vertical"));
        protected override void FixedUpdate() {
            transform.localRotation = Quaternion.Euler(
                x: Mathf.Clamp(transform.localEulerAngles.x+mouse.y*10,-60,60),
                y: transform.localEulerAngles.y+mouse.x*10, z: 0);
            base.FixedUpdate();
        }

        async void OnJump() { await 1; Manager.LoadScene("Moon Base Delta"); }
        async void OnKill() { await 8; PlayerCamera.Reset(); Manager.LoadMenu(); await 5; }
    }
}

