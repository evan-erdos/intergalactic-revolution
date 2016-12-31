/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace Adventure.Astronomy.Aeronautics {
    public class SpaceshipController : SpaceObject {
        bool brakes, boost, blaster, missile, changeMode;
        int next = 1;
        float roll, pitch, yaw, spin, steep, throttle;
        float aerodynamicEffect, dragCoefficient;
        [SerializeField] protected Spaceship spaceship;
        List<FlightMode> list = new List<FlightMode> {
            FlightMode.Assisted, FlightMode.Directed, FlightMode.Manual};

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
            throttle = CrossPlatformInputManager.GetAxis("Throttle");
            //throttle = CrossPlatformInputManager.GetAxis("_joy_axis4");
            // print(throttle);
            // if (0==throttle) throttle = CrossPlatformInputManager.GetAxis("_joy_axis5");
            // if (0==throttle) throttle = CrossPlatformInputManager.GetAxis("_joy_axis6");
            // if (0==throttle) throttle = CrossPlatformInputManager.GetAxis("_joy_axis7");
            //var throt = CrossPlatformInputManager.GetAxis("Throttle");
            // print(throttle);
            // if (0.01f<Mathf.Abs(throt)) Shift(throt*((0<throt)?1:-2));
            // if (CrossPlatformInputManager.GetAxis("Downshift")) Shift(-20);
            // if (CrossPlatformInputManager.GetButton("Upshift")) Shift(10);
            if (CrossPlatformInputManager.GetButton("ModeUp")) ChangeMode(true);
            if (CrossPlatformInputManager.GetButton("ModeDown")) ChangeMode(false);
        }

        void FixedUpdate() {
            if (missile) spaceship.FireRockets();
            if (blaster) spaceship.Fire();
            spaceship.Move(brakes,boost,roll,pitch,yaw,steep,throttle,spin);
        }

        void Shift(float delta) {
            // StartSemaphore(Shifting);
            StartCoroutine(Shifting(delta));
        }

        IEnumerator Shifting(float delta) {
            throttle = Mathf.Max(Mathf.Min(throttle+delta*20,4),-8);
            yield return new WaitForSeconds(0.01f);
            throttle = 0;
        }

        void ChangeMode(bool isUp) {
            StartSemaphore(Moding);
            IEnumerator Moding() {
                next += (isUp)?1:-1;
                if (0>next) next = list.Count-1;
                spaceship.Mode = list[next % list.Count];
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
