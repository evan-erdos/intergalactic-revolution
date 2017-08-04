/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
// using Input=UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class SpacePlayer : SpaceActor {
        FlightArgs e = new FlightArgs();
        [SerializeField] PilotProfile profile;
        public override void OnStartLocalPlayer() => DontDestroyOnLoad(gameObject);
        protected override void FixedUpdate() { base.FixedUpdate(); Ship?.Move(e); }
        void Update() { // if (isLocalPlayer) ControlInput(); }
            (e.Sender, e.Thrust) = (this, Input.GetAxis("Thrust")-(Input.GetButton("Drag")?2:0));
            (e.Roll, e.Pitch, e.Yaw) = (Input.GetAxis("Roll"), Input.GetAxis("Pitch"), Input.GetAxis("Yaw"));
            (e.Lift, e.Strafe, e.Spin) = (Input.GetAxis("Lift"), Input.GetAxis("Strafe"), Input.GetButton("Spin")?1:0);
            if (Input.GetButton("Fire") || 0.8<Input.GetAxis("Attack")) Ship?.Fire();
            if (Input.GetButton("Jump")) Ship?.HyperJump();
            if (Input.GetButtonDown("Target")) Ship?.SelectTarget();
            if (Input.GetButtonDown("Select")) Ship?.SelectSystem();
            if (Input.GetAxis("Cycle")>0) Ship?.SelectWeapon();
            if (Input.GetAxis("Cycle")<0) Ship?.ToggleView();
            if (Input.GetButton("Shift")) (e.Roll, e.Yaw) = (0, Input.GetAxis("Roll"));
            else (e.Lift, e.Strafe) = (0,0);
        }

        public override void SetShip(Spaceship o) {
            (Ship, PlayerCamera.Target) = (o, o.transform); Ship?.ToggleView();
            Ship.KillEvent += e => OnKill(); Ship.JumpEvent += e => OnJump(e); }

        // void OnNetworkInstantiate(NetworkMessageInfo info) => CreateShip();
        async void OnJump(TravelArgs e) { await 1; Manager.Jump(e.Destination); }
        async void OnKill() { await 8; PlayerCamera.Reset(); Manager.LoadMenu(); await 5; }
    }
}




