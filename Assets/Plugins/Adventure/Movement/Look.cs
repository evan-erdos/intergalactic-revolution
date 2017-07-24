/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-07-11 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Movement {
    public class Look : MonoBehaviour {
        uint ind, size = 8;
        float ratio;
        public enum rotAxes { MouseXY, MouseX, MouseY }
        public float speed = 2;
        public rotAxes rotAxis = rotAxes.MouseXY;
        Vector2 Sensitivity, cr, a; // current rotation, a
        Vector2[] rarr; // rotation array
        Vector3 pr; // previous rotation
        Vector4 Maxima;
        Quaternion d, lr; // delta rotation, last rotation

        void Awake() {
            (ratio,rarr) = (Screen.width/Screen.height, new Vector2[(int)size]);
            Maxima.Set(-360,360,-90,90);
            lr = transform.localRotation;
            if (GetComponent<Rigidbody>() is Rigidbody r) r.freezeRotation = true;
            Sensitivity.Set(speed*ratio, speed);
        }

        void FixedUpdate() {
            if (Input.GetMouseButtonDown(0))
                (Cursor.visible, Cursor.lockState) = (false, CursorLockMode.Locked);
            pr = Vector3.zero;
            a.Set(0,0);
            cr.x += Input.GetAxis("Mouse X")*Sensitivity.x;
            cr.y += Input.GetAxis("Mouse Y")*Sensitivity.y;
            cr.y = ClampAngle(cr.y, Maxima.z, Maxima.w);
            rarr[(int)ind] = cr;
            foreach (var i in rarr) a += i;
            a /= (int)size;
            a.y = ClampAngle(a.y, Maxima.z, Maxima.w);
            switch (rotAxis) {
                case rotAxes.MouseXY : d = Quaternion.Euler(-a.y,a.x,pr.z); break;
                case rotAxes.MouseX : d = Quaternion.Euler(-pr.y,a.x,pr.z); break;
                case rotAxes.MouseY : d = Quaternion.Euler(-a.y,pr.x,pr.z); break;
            } ind++;
            transform.localRotation = lr*d;
            if ((int)ind >= (int)size) ind -= size;
        }

        static float ClampAngle(float delta, float maximaL, float maximaH) {
            delta %= 360;
            if (delta<=-360) delta += 360;
            else if (delta>=360) delta -= 360;
            return Mathf.Clamp(delta, maximaL, maximaH);
        }
    }
}
