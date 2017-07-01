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
        void Update() => ControlInput(); // If(isLocalPlayer, () => ControlInput());
        void ControlInput() {
            // foreach (KeyCode key in Enum.GetValues(typeof(KeyCode))) if (Input.GetKeyDown(key)) print(key);
            (roll, pitch, yaw) = (Input.GetAxis("Roll"), Input.GetAxis("Pitch"), Input.GetAxis("Yaw"));
            (brake, boost, speed) = (Input.GetButton("Brake")?1:0, Input.GetButton("Action")?1:0, Input.GetAxis("Speed"));
            if (GetBoost()>0) boost = GetBoost();
            if (GetAttack()>0.8 || Input.GetButton("Fire")) Ship?.Fire();
            if (Input.GetButton("Jump")) Ship?.HyperJump(); // Ship?.SelectSystem();
            if (Input.GetButtonDown("Select")) Ship?.SelectTarget();
            if (Input.GetAxis("Cycle")>0) Ship?.SelectWeapon();
            if (Input.GetAxis("Cycle")<0) Ship?.ChangeMode();
            if (Input.GetAxis("Mode")>0) Ship?.ToggleView();

            float GetBoost() => (Input.GetAxis("Attack")>0)?Input.GetAxis("Attack"):0;
            float GetAttack() => (Input.GetAxis("Attack")<0)?-Input.GetAxis("Attack"):0;
        }
    }
}
