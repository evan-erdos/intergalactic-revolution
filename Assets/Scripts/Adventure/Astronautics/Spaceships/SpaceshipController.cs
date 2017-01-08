/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityStandardAssets.CrossPlatformInput;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class SpaceshipController : SpaceObject {
        enum PlayerNumber { Player1, Player2 };
        bool brakes, boost, jump, weapon, toggle;
        float roll, pitch, yaw, spin, throttle;
        public GameObject miniHUD, fullHUD;
        public Spaceship Ship {get;set;}
        [SerializeField] PlayerNumber player = PlayerNumber.Player1;
        Map<PlayerNumber,Map<string>> keynames = new Map<PlayerNumber,Map<string>> {
            [PlayerNumber.Player1] = new Map<string> {
                ["roll"] = "Roll", ["pitch"] = "Pitch",
                ["yaw"] = "Yaw", ["throttle"] = "Throttle",
                ["brake"] = "Brake", ["boost"] = "Boost",
                ["jump"] = "Jump", ["weapon"] = "Fire2",
                ["cycle"] = "Fire3", ["mode"] = "Mode",
                ["toggle"] = "Toggle", ["select"] = "Select"},
            [PlayerNumber.Player2] = new Map<string> {
                ["roll"] = "Roll 2", ["pitch"] = "Pitch 2",
                ["yaw"] = "Yaw 2", ["throttle"] = "Throttle 2",
                ["brake"] = "Brake 2", ["boost"] = "Boost 2",
                ["jump"] = "Jump 2", ["weapon"] = "Fire2 2",
                ["cycle"] = "Fire3 2", ["mode"] = "Mode 2",
                ["toggle"] = "Toggle 2", ["select"] = "Select 2"}};

        void Update() {
            brakes = Input.GetButton(keynames[player]["brake"]);
            boost = Input.GetButton(keynames[player]["boost"]);
            roll = Input.GetAxis(keynames[player]["roll"]);
            pitch = Input.GetAxis(keynames[player]["pitch"]);
            yaw = Input.GetAxis(keynames[player]["yaw"]);
            throttle = Input.GetAxis(keynames[player]["throttle"]);
            if (Input.GetButton(keynames[player]["jump"])) Ship.HyperJump();
            if (Input.GetButton(keynames[player]["weapon"])) Ship.Fire();
            if (Input.GetButtonDown(keynames[player]["cycle"])) Ship.SelectWeapon();
            if (Input.GetButtonDown(keynames[player]["mode"])) Ship.ChangeMode();
            if (Input.GetButton(keynames[player]["toggle"])) Toggle();
            if (Input.GetButton(keynames[player]["select"])) Ship.Select();
            // foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
            //     if (Input.GetKeyDown(code)) print(code);
        }

        void FixedUpdate() => Ship.Move(brakes,boost,throttle,roll,pitch,yaw);

        void Toggle() {
            StartSemaphore(Toggling);
            IEnumerator Toggling() {
                toggle = !toggle;
                miniHUD.SetActive(!toggle);
                fullHUD.SetActive(toggle);
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}
