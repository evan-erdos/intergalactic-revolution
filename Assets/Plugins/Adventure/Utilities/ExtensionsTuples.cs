/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-14 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class Extensions {
    public static void Deconstruct(this Vector3 o, out float x, out float y, out float z) => (x,y,z) = (o.x, o.y, o.z);
    public static void Deconstruct(this (float x,float y,float z) o, out Vector3 v) => v = new Vector3(o.Item1,o.Item2,o.Item3);
    public static void Deconstruct(this Transform o, out Vector3 position, out Quaternion rotation) => (position,rotation) = (o.position,o.rotation);
    public static void AddForce(this Rigidbody o, (float,float,float) force, ForceMode mode=ForceMode.Force) => o.AddForce(force.vect(), mode);
    public static void AddTorque(this Rigidbody o, (float,float,float) force, ForceMode mode=ForceMode.Force) => o.AddTorque(force.vect(), mode);
    public static float Angle(this Vector3 o, Vector3 v) => Vector3.Angle(o,v);
    public static float Angle(this Vector3 o, (float,float,float) v) => o.Angle(v.vect());
    public static float Angle(this (float,float,float) o, Vector3 v) => o.vect().Angle(v);
    public static float Angle(this (float,float,float) o, (float,float,float) v) => o.vect().Angle(v.vect());
    public static float magnitude(this (float,float,float) o) => o.vect().magnitude;
    public static float sqrMagnitude(this (float,float,float) o) => o.vect().sqrMagnitude;
    public static (float x,float y,float z) tuple(this Vector3 o) => (o.x,o.y,o.z);
    public static Vector3 vect(this (float,float,float) o) => new Vector3(x: o.Item1, y: o.Item2, z: o.Item3);
    public static Vector3 normalized(this (float,float,float) o) => o.vect().normalized;
}
