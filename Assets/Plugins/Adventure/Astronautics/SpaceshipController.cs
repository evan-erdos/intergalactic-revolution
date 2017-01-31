/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Input=UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class SpaceshipController : Adventure.NetObject {
        float brake, boost, speed, roll, pitch, yaw;
        SpacePlayer player;
        public Spaceship Ship => player.Ship;
        public override void OnStartLocalPlayer() => DontDestroyOnLoad(gameObject);
        void Awake() => player = Get<SpacePlayer>();
        void Start() => Ship?.ToggleView();
        void FixedUpdate() => Ship?.Move(brake,boost,speed,roll,pitch,yaw);
        void Update() => If(true, () => ControlInput());
        // void Update() => If(isLocalPlayer, () => ControlInput());
        void ControlInput() {
            (roll, pitch) = (Input.GetAxis("Roll"),Input.GetAxis("Pitch"));
            (yaw, speed) = (Input.GetAxis("Yaw"), Input.GetAxis("Speed"));
            brake = Input.GetButton("Brake")?1:0;
            boost = Input.GetButton("Boost")?1:0;
            // // for xbox controllers
            // (roll, pitch) = (Input.GetAxis("Roll"),Input.GetAxis("Pitch"));
            // yaw = Input.GetAxis("Yaw");
            // (speed,boost) = (Input.GetAxis("Speed"),Input.GetAxis("Boost"));
            // if (boost<0) (boost,brake) = (0,boost);
            if (Input.GetButton("Jump")) Ship?.HyperJump();
            if (Input.GetButton("Fire")) Ship?.Fire();
            if (Input.GetButtonDown("Switch")) Ship?.SelectWeapon();
            if (Input.GetButtonDown("Mode")) Ship?.ChangeMode();
            if (Input.GetButtonDown("Toggle")) Ship?.ToggleView();
            if (Input.GetButtonDown("Select")) Ship?.SelectTarget();
            if (Input.GetButtonDown("Hyperspace")) Ship?.SelectSystem();
        }
    }
}
