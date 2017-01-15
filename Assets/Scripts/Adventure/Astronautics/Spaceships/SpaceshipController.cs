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
        void Update() => If(isLocalPlayer, () => ControlInput());
        void ControlInput() {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            // for use with xbox controllers
            (roll, pitch) = (Input.GetAxis("Roll"),Input.GetAxis("Pitch"));
            yaw = Input.GetAxis("Yaw");
            speed = Input.GetAxis("Speed");
            boost = Input.GetAxis("Boost");
            if (boost<0) (boost,brake) = (0,boost);
            if (Input.GetButton("Jump")) Ship.HyperJump();
            if (Input.GetButton("Fire")) Ship.Fire();
            if (Input.GetButtonDown("Switch")) Ship.SelectWeapon();
            if (Input.GetButtonDown("Mode")) Ship.ChangeMode();
            if (Input.GetButtonDown("Toggle")) Ship.ToggleView();
            if (Input.GetButtonDown("Select")) Ship.SelectTarget();
#else
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
#endif
        }
    }
}
