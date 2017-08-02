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
        void Start() { e.Sender = this; Ship?.ToggleView(); }
        public override void OnStartLocalPlayer() => DontDestroyOnLoad(gameObject);
        protected override void FixedUpdate() { base.FixedUpdate(); Ship?.Move(e); }
        void Update() => ControlInput(); // if (isLocalPlayer) ControlInput(); }
        void ControlInput() { // Manager.PrintInput();
            e.Thrust = Input.GetAxis("Thrust")-(Input.GetButton("Drag")?2:0);
            // e.Spin = Input.GetButton("Mode")?e.Pitch:0;
            (e.Roll, e.Pitch, e.Yaw) = (Input.GetAxis("Roll"), Input.GetAxis("Pitch"), Input.GetAxis("Yaw"));
            if (Input.GetButton("Fire") || 0.8<Input.GetAxis("Attack")) Ship?.Fire();
            if (Input.GetButton("Jump")) Ship?.HyperJump();
            if (Input.GetButtonDown("Target")) Ship?.SelectTarget();
            if (Input.GetButtonDown("Select")) Ship?.SelectSystem();
            if (Input.GetAxis("Cycle")>0) Ship?.SelectWeapon();
            if (Input.GetAxis("Cycle")<0) Ship?.ToggleView();
            if (Input.GetButton("Shift")) (e.Roll, e.Yaw, e.Lift, e.Strafe) =
                (0, Input.GetAxis("Roll"), Input.GetAxis("Lift"), Input.GetAxis("Strafe"));
            else (e.Lift, e.Strafe) = (0,0);
        }

        public override void SetShip(Spaceship o) {
            (Ship, PlayerCamera.Target) = (o, o.transform);
            Ship.KillEvent += e => OnKill();
            Ship.JumpEvent += e => OnJump(e);
        }

        // public override void OnStartLocalPlayer() => CreateShip();
        // void OnNetworkInstantiate(NetworkMessageInfo info) => CreateShip();
        async void OnJump(TravelArgs e) { await 1; Manager.Jump(e.Destination); }
        async void OnKill() { await 8; PlayerCamera.Reset(); Manager.LoadMenu(); await 5; }
    }
}




