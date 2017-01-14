/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Input=UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class SpaceshipController : NetworkSpaceObject {
        float brake, boost, speed, roll, pitch, yaw;
        public Spaceship Ship {get;set;}
        void Start() => Ship.ToggleView();
        void FixedUpdate() => Ship.Move(brake,boost,speed,roll,pitch,yaw);
        void Update() {
            if (!isLocalPlayer) return;
            float AsAxis(string key) => Input.GetButton(key)?1:0;
            (roll, pitch) = (Input.GetAxis("Roll"),Input.GetAxis("Pitch"));
            (yaw, speed) = (Input.GetAxis("Yaw"), Input.GetAxis("Speed"));
            (brake, boost) = (AsAxis("Brake"), AsAxis("Boost"));
            if (Input.GetButton("Jump")) Ship.HyperJump();
            if (Input.GetButton("Fire")) Ship.Fire();
            if (Input.GetButtonDown("Switch")) Ship.SelectWeapon();
            if (Input.GetButtonDown("Mode")) Ship.ChangeMode();
            if (Input.GetButtonDown("Toggle")) Ship.ToggleView();
            if (Input.GetButtonDown("Select")) Ship.SelectTarget();
        }
    }
}
