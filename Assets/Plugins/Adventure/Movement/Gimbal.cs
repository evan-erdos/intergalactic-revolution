/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-07-01 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Movement {
    public class Gimbal : MonoBehaviour {
        bool wasHit, isRestoring = true;
        uint index;
        int maskPlayer;
        const int iter = 16;
        float
            roll,
            angle = 50f,
            curL = 0f,
            maxL = 0.5f,
            timeT = 0.8f,
            inv = 1.0f/iter;
        public float
            modSprint = 2.1f,
            modCrouch = 3.5f,
            thetaL = 0f,
            thetaT = 50f;
        double sigma, cutoff = 0.001f;
        double[] rotArray = new double[iter];
        IMotor motor;
        RaycastHit hitSphere;
        public GameObject childPlayer;
        Transform mCamera;
        Vector3 motorRotation, initCam;
        public Transform deadPlayer, mapPlayer, deadtemp;

        void Awake() {
            childPlayer = GameObject.FindWithTag("Player");
            maskPlayer = LayerMask.NameToLayer("Player");
            motor = childPlayer.GetComponent<Motor>();
            mCamera = Camera.main.transform;
        }


        void Update() => roll = Input.GetAxis("Roll");

        public void FixedUpdate() {
            if (Mathf.Abs(Time.deltaTime)<0.01f) return;
            childPlayer.transform.parent = null;
            transform.position = childPlayer.transform.position;
            childPlayer.transform.parent = transform;
            rotArray[(int)index%iter] = roll*angle*Time.smoothDeltaTime;
            foreach (var entry in rotArray) sigma += entry;
            sigma *= inv*((motor.IsSprinting)?modSprint:1.0f);
            index = (uint)(index%iter)+1;
            if (motor.IsGrounded) {
                initCam = transform.position+Vector3.up*1.8f;
                maxL = ((motor.IsSprinting)?modSprint*0.5f:0.5f);
                if (Mathf.Abs(roll)>float.Epsilon) {
                    wasHit = Physics.SphereCast(
                        initCam, 0.4f,
                        initCam+mCamera.TransformDirection(
                            Vector3.right)*maxL*roll,
                        out hitSphere, 1f, ~maskPlayer);
                    maxL = Mathf.Min(maxL,(wasHit)?hitSphere.distance:maxL);
                    if (Mathf.Abs(thetaL-maxL)>0.1f)
                        thetaL = Mathf.SmoothDampAngle(
                            thetaL, roll*maxL,
                            ref curL, 0.1f, 5f, Time.fixedDeltaTime);
                } else thetaL = Mathf.SmoothDampAngle(
                    thetaL, 0, ref curL, 0.1f, 5f, Time.fixedDeltaTime);
                motorRotation.Set(thetaL, mCamera.localPosition.y, 0);
                mCamera.localPosition = motorRotation;
                isRestoring = true;
            } if (isRestoring) {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.identity,
                    timeT*Time.smoothDeltaTime*8);
                isRestoring = transform.rotation != Quaternion.identity;
                if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.x, 0))<cutoff
                || Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, 0))<cutoff
                || Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, 0))<cutoff)
                    transform.rotation = Quaternion.identity;
            } if (!motor.IsGrounded)
                transform.RotateAround(
                    childPlayer.transform.position,
                    childPlayer.transform.forward, (float)sigma);
        }
    }
}
