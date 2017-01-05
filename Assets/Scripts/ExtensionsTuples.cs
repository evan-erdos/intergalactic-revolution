/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-03 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionsTuples {

    public static (float x, float y, float z) ToTuple(this Vector3 o) => (o.x, o.y, o.z);
    public static Vector3 ToVector(this (float,float,float) o) =>
        new Vector3(x: o.Item1, y: o.Item2, z: o.Item3);

    public static void Deconstruct(
                    this Vector3 o,
                    out float x,
                    out float y,
                    out float z) =>
        (x,y,z) = (o.x, o.y, o.z);

    public static void Deconstruct(
                    this (float x, float y, float z) o,
                    out Vector3 v) =>
        v = new Vector3(o.x, o.y, o.z);

    public static void Deconstruct(
                    this (double x, double y, double z) o,
                    out Vector3 v) =>
        v = new Vector3((float) o.x, (float) o.y, (float) o.z);

    // public static void Deconstruct(
    //                 this (float, float, float) o,
    //                 out Vector3 v) =>
    //     v = new Vector3(o.Item1, o.Item2, o.Item3);

    // public static void Deconstruct(
    //                 this (double, double, double) o,
    //                 out Vector3 v) =>
    //     v = new Vector3((float) o.Item1, (float) o.Item2, (float) o.Item3);

    public static void Deconstruct(
                    this Transform o,
                    out Vector3 position,
                    out Quaternion rotation) =>
        (position, rotation) = (o.position, o.rotation);


}
