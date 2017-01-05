/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class SpaceshipController : SpaceObject {
        bool brakes, boost, blaster, missile, changeMode;
        float roll, pitch, yaw, spin, steep, throttle;
        float aerodynamicEffect, dragCoefficient;
        public Spaceship Ship {get;set;}
        List<FlightMode> list = new List<FlightMode> {
            FlightMode.Navigation, FlightMode.Assisted, FlightMode.Manual};

        void Update() {
            roll = CrossPlatformInputManager.GetAxis("Horizontal");
            pitch = CrossPlatformInputManager.GetAxis("Vertical");
            yaw = CrossPlatformInputManager.GetAxis("Yaw");
            spin = CrossPlatformInputManager.GetAxis("Spin");
            steep = CrossPlatformInputManager.GetAxis("Steep");
            brakes = CrossPlatformInputManager.GetButton("Brake");
            boost = CrossPlatformInputManager.GetButton("Boost");
            blaster = CrossPlatformInputManager.GetButton("Fire2");
            missile = CrossPlatformInputManager.GetButton("Fire3");
            throttle = CrossPlatformInputManager.GetAxis("Throttle")*4;
            if (CrossPlatformInputManager.GetButton("ModeUp")) ChangeMode(true);
            if (CrossPlatformInputManager.GetButton("ModeDown")) ChangeMode(false);
        }

        void FixedUpdate() {
            if (missile) Ship.FireRockets();
            if (blaster) Ship.Fire();
            Ship.Move(brakes,boost,roll,pitch,yaw,steep,throttle,spin);
        }

        void Shift(float delta) {
            StartSemaphore(Shifting);
            IEnumerator Shifting() {
                throttle = Mathf.Max(Mathf.Min(throttle+delta*20,4),-8);
                yield return new WaitForSeconds(0.01f);
                throttle = 0;
            }
        }

        int next = 1; // on the way out
        void ChangeMode(bool isUp) {
            StartSemaphore(Moding);
            IEnumerator Moding() {
                next += (isUp)?1:-1;
                if (0>next) next = list.Count-1;
                Ship.Mode = list[next % list.Count];
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
