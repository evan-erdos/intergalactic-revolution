/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
// using Input=UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class SpacePlayer : SpaceActor {
        static bool isVR = true;
        static string vrTouch = " VR", vr = "";
        FlightArgs e = new FlightArgs();
        [SerializeField] PilotProfile profile;
        void Start() => vr = isVR?vrTouch:"";
        public override void OnStartLocalPlayer() => DontDestroyOnLoad(gameObject);
        protected override void FixedUpdate() { base.FixedUpdate(); Ship?.Move(e); }
        void Update() { // if (isLocalPlayer) ControlInput(); }
            (e.Sender, e.Thrust) = (this, GetAxis("Thrust")-(GetButton("Drag")?2:0));
            (e.Roll, e.Pitch, e.Yaw) = (GetAxis("Roll"), GetAxis("Pitch"), GetAxis("Yaw"));
            (e.Lift, e.Strafe, e.Spin) = (GetAxis("Lift"), GetAxis("Strafe"), GetAxis("Spin"));
            if (GetButton("Fire") || 0.1<GetAxis("Attack")) Ship?.Fire();
            if (GetButton("Jump")) Ship?.HyperJump();
            if (GetButtonDown("Target")) Ship?.SelectTarget();
            // if (Input.GetButtonDown("Select")) Ship?.SelectSystem();
            if (GetButton("Shift")) (e.Roll, e.Yaw) = (0, GetAxis("Roll"));
            else (e.Lift, e.Strafe) = (0,0); // Manager.PrintInput();

            float GetAxis(string o) => Input.GetAxis($"{o}{vr}");
            bool GetButton(string o) => Input.GetButton($"{o}{vr}");
            bool GetButtonDown(string o) => Input.GetButtonDown($"{o}{vr}");
        }

        public override void SetShip(Spaceship o) {
            (Ship, PlayerCamera.Target) = (o, o.transform); Ship?.ToggleView();
            Ship.KillEvent += e => OnKill(); Ship.JumpEvent += e => OnJump(e); }

        // void OnNetworkInstantiate(NetworkMessageInfo info) => CreateShip();
        async void OnJump(TravelArgs e) { await 1; Manager.Jump(e.Destination); }
        async void OnKill() { await 8; PlayerCamera.Reset(); Manager.LoadMenu(); await 5; }
    }
}




