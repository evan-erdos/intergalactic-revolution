/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace Adventure.Astronomy {

    /// FlightMode
    /// defines the different kinds of flight control systems
    public enum FlightMode { Assisted, Directed, Manual };

    /// SpaceArgs : EventArgs
    /// provides a base argument type for space events
    public class SpaceArgs : EventArgs { }

    /// SpaceEvent : UnityEvent
    /// a serializable event handler to expose to the editor
    [Serializable]
    public class SpaceEvent : UnityEvent<ISpaceObject,SpaceArgs> { }

    /// SpaceAction : (sender, args) => void
    public delegate void SpaceAction(ISpaceObject sender, SpaceArgs args);

    /// Units
    /// all the constants of the universe
    public static class Units {
        public const int Day = 86400; // sec
        public const int km = 1; // m
        public const double AU = 149597870700.0; // km
        public const float Time = 3600f; // sec
        public const int Seed = 688067; // Day
        public static DateTime Date = new DateTime(1885,1,12); // Date
    }


    public static class Extensions {

        public static string Ellipsis(this string s, int len=100) {
            return (s.Length<len) ? s : s.Substring(0,len-1)+"…"; }

        public static Color32 ToColor(this int hex) { return new Color32(
            (byte)((hex >> 16) & 0xFF),
            (byte)((hex >> 8) & 0xFF),
            (byte)(hex & 0xFF), (0xFF)); }

        public static int ToInt(this Color32 color) { return
            (byte)((color.r >> 16) & 0xFF) +
            (byte)((color.g >> 8) & 0xFF) +
            (byte)(color.b & 0xFF) + (0xFF); }

        public static Color32 ToColor(this string s) { return new Color32(
            (byte)((System.Convert.ToInt32(s.Substring(1,3),16)>>16)&0xFF),
            (byte)((System.Convert.ToInt32(s.Substring(4,6),16)>>8)&0xFF),
            (byte)(System.Convert.ToInt32(s.Substring(7,9),16)&0xFF),(0xFF)); }

        public static bool IsNear(
                        this Vector3 o,
                        Vector3 other,
                        float dist=float.Epsilon) {
            return (o-other).sqrMagnitude<dist*dist; }


        public static bool IsNear(
                        this Transform o,
                        Transform other,
                        float dist=float.Epsilon) {
            return o.position.IsNear(other.position,dist); }


        public static float Distance(this Transform o, Transform other) {
            return (o.position-other.position).magnitude; }


        public static float Distance(this Vector3 o, Vector3 other) {
            return (o-other).magnitude; }


        public static T Create<T>(GameObject original) where T : Component {
            return Create<T>(original, Vector3.zero, Quaternion.identity); }


        public static T Create<T>(
                        GameObject original,
                        Vector3 position)
                        where T : Component {
            return Create<T>(original, position, Quaternion.identity); }


        public static T Create<T>(
                        GameObject original,
                        Vector3 position,
                        Quaternion rotation) where T : Component {
            var instance = UnityEngine.Object.Instantiate(
                original: original,
                position: position,
                rotation: rotation) as GameObject;
            return instance.GetComponent<T>();
        }

        public static T GetChild<T>(this GameObject o, string s) {
            foreach (Transform child in o.transform)
                if (child.tag == s) return child.GetComponent<T>();
            return default(T);
        }

        public static IEnumerable<T> GetChildren<T>(
                        this GameObject gameObject,
                        string tag) =>
            from Transform child in gameObject.transform
            where child.tag==tag
            select child.GetComponent<T>();
    }
}
