/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
// using Input=UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class SpacePlayer : SpaceActor {
        FlightArgs args = new FlightArgs();
        public override void OnStartLocalPlayer() => DontDestroyOnLoad(gameObject);
        void Start() { args.Sender = this; Ship?.ToggleView(); }
        protected override void FixedUpdate() { base.FixedUpdate(); Ship?.Move(args); }
        void Update() => ControlInput(); // if (isLocalPlayer) ControlInput();
        void ControlInput() { // Manager.PrintInput();
            (args.Roll, args.Yaw) = (Input.GetAxis("Roll"), Input.GetAxis("Yaw"));
            (args.Pitch, args.Turbo) = (Input.GetAxis("Pitch"), Input.GetButton("Action")?1:0);
            (args.Lift, args.Strafe) = (Input.GetAxis("Lift"), Input.GetAxis("Strafe"));
            args.Thrust = Input.GetAxis("Thrust")-(Input.GetButton("Drag")?2:0);
            if (Input.GetButton("Fire") || 0.7<Input.GetAxis("Attack")) Ship?.Fire();
            if (Input.GetButton("Jump")) Ship?.HyperJump();
            if (Input.GetButtonDown("Target")) Ship?.SelectTarget();
            if (Input.GetButtonDown("Select")) Ship?.SelectSystem();
            if (Input.GetAxis("Cycle")>0) Ship?.SelectWeapon();
            if (Input.GetAxis("Cycle")<0) Ship?.ChangeMode();
            if (Input.GetButtonDown("Mode")) Ship?.ToggleView();
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




