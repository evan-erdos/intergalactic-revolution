/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Input=UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class SpaceshipController : NetworkSpaceObject {
        bool toggle;
        float brake, boost, speed, roll, pitch, yaw;
        public Spaceship Ship {get;set;}
        List<(float fov,(float x,float y,float z))> pivots =
            new List<(float fov,(float x,float y,float z))> {
                (fov: 40, (x: 0, y: 0.5f, z: -0.25f)),
                (fov: 60, (x: 0, y: 4, z: -20))};

        void Start() => ChangeCamera(false);
        void FixedUpdate() => Ship.Move(brake,boost,speed,roll,pitch,yaw);
        void Update() {
            if (!isLocalPlayer) return;
            float GetAxis(string key) => Input.GetAxis(key);
            float AsAxis(string key) => Input.GetButton(key)?1:0;
            (brake,boost,speed) = (AsAxis("Brake"),AsAxis("Boost"),GetAxis("Speed"));
            (roll,pitch,yaw) = (GetAxis("Roll"),GetAxis("Pitch"),GetAxis("Yaw"));
            if (Input.GetButton("Jump")) Ship.HyperJump();
            if (Input.GetButton("Fire")) Ship.Fire();
            if (Input.GetButtonDown("Switch")) Ship.SelectWeapon();
            if (Input.GetButtonDown("Mode")) Ship.ChangeMode();
            if (Input.GetButtonDown("Toggle")) Toggle();
            if (Input.GetButtonDown("Select")) Ship.Select();
        }

        void ChangeCamera(bool toggle) {
            var (fov,(x,y,z)) = pivots[toggle?1:0];
            var camera = GetComponentInChildren<Camera>();
            camera.fieldOfView = fov;
            camera.transform.localPosition = (x,y,z).ToVector();
        }

        bool wait;
        void Toggle() {
            StartCoroutine(Toggling());
            // StartSemaphore(Toggling);
            IEnumerator Toggling() {
                if (wait) yield break;
                wait = true;
                ChangeCamera(toggle = !toggle);
                yield return new WaitForSeconds(0.2f);
                wait = false;
            }
        }
    }
}
