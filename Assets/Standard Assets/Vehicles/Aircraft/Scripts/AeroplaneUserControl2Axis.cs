using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Aeroplane {

    [RequireComponent(typeof (AeroplaneController))]
    public class AeroplaneUserControl2Axis : MonoBehaviour {

        AeroplaneController aeroplane;
        bool wait, brakes;
        float roll, pitch, yaw, throttle;


        void Awake() { aeroplane = GetComponent<AeroplaneController>(); }

        void Update() {
            if (CrossPlatformInputManager.GetButton("Upshift")) Upshift();
            if (CrossPlatformInputManager.GetButton("Downshift")) Downshift();
            roll = CrossPlatformInputManager.GetAxis("Horizontal");
            pitch = CrossPlatformInputManager.GetAxis("Vertical");
            yaw = CrossPlatformInputManager.GetAxis("Yaw");
            brakes = CrossPlatformInputManager.GetButton("Brake");
        }


        void Upshift() { if (!wait) StartCoroutine(Shifting(throttle+0.1f)); }

        void Downshift() { if (!wait) StartCoroutine(Shifting(throttle-0.1f)); }


        IEnumerator Shifting(float speed) {
            wait = true;
            throttle = Mathf.Max(Mathf.Min(speed,2),0);
            yield return new WaitForSeconds(0.1f);
            wait = false;
        }


        void FixedUpdate() { aeroplane.Move(
            roll: roll,
            pitch: pitch,
            yaw: yaw,
            throttle: throttle,
            airBrakes: brakes); }
    }
}
