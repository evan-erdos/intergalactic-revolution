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
        bool brakes, boost, weapon, toggle;
        float roll, pitch, yaw, spin, throttle;
        public GameObject miniHUD, fullHUD;
        public Spaceship Ship {get;set;}
        List<FlightMode> list = new List<FlightMode> {
            FlightMode.Navigation, FlightMode.Assisted, FlightMode.Manual};
        [SerializeField] PlayerNumber player = PlayerNumber.Player1;
        Map<PlayerNumber,Map<string>> keynames = new Map<PlayerNumber,Map<string>> {
            [PlayerNumber.Player1] = new Map<string> {
                ["roll"] = "Roll", ["pitch"] = "Pitch",
                ["yaw"] = "Yaw", ["spin"] = "Spin",
                ["brake"] = "Brake", ["boost"] = "Boost",
                ["throttle"] = "Throttle", ["weapon"] = "Fire2",
                ["cycle"] = "Fire3", ["mode"] = "Mode", ["toggle"] = "Toggle"},
            [PlayerNumber.Player2] = new Map<string> {
                ["roll"] = "Roll 2", ["pitch"] = "Pitch 2",
                ["yaw"] = "Yaw 2", ["spin"] = "Spin 2",
                ["brake"] = "Brake 2", ["boost"] = "Boost 2",
                ["throttle"] = "Throttle 2", ["weapon"] = "Fire2 2",
                ["cycle"] = "Fire3 2", ["mode"] = "Mode 2", ["toggle"] = "Toggle 2"}};

        void Update() {
            roll = Input.GetAxis(keynames[player]["roll"]);
            pitch = Input.GetAxis(keynames[player]["pitch"]);
            yaw = Input.GetAxis(keynames[player]["yaw"]);
            spin = Input.GetAxis(keynames[player]["spin"]);
            brakes = Input.GetButton(keynames[player]["brake"]);
            boost = Input.GetButton(keynames[player]["boost"]);
            throttle = Input.GetAxis(keynames[player]["throttle"]);
            if (Input.GetButton(keynames[player]["weapon"])) Ship.Fire();
            if (Input.GetButton(keynames[player]["cycle"])) Ship.SelectWeapon();
            if (Input.GetButton(keynames[player]["mode"])) Mode();
            if (Input.GetButton(keynames[player]["toggle"])) Toggle();
        }

        void FixedUpdate() => Ship.Move(brakes,boost,roll,pitch,yaw,throttle);
        void Fire() => Ship.Fire();

        void Shift(float delta) {
            StartSemaphore(Shifting);
            IEnumerator Shifting() {
                throttle = Mathf.Max(Mathf.Min(throttle+delta*20,4),-8);
                yield return new WaitForSeconds(0.01f);
                throttle = 0;
            }
        }

        int next = 1; // on the way out
        void Mode(bool isUp=true) {
            StartSemaphore(Moding);
            IEnumerator Moding() {
                next += (isUp)?1:-1;
                if (0>next) next = list.Count-1;
                Ship.Mode = list[next % list.Count];
                yield return new WaitForSeconds(0.2f);
            }
        }

        void Cycle() {
            StartSemaphore(Switching);
            IEnumerator Switching() {
                Ship.SelectWeapon();
                yield return new WaitForSeconds(0.2f);
            }
        }

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
