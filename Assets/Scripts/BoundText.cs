/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using ui=UnityEngine.UI;
using System.Collections;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

class BoundText : MonoBehaviour {
    // int[] velocities = new[] {
    //     0x11FF00, 0x11FF00, 0x22DD00, 0x22DD00,
    //     0x44BB00, 0x669900, 0x886600, 0xAA4400,
    //     0xCC2200, 0xFF0000, 0xFF0000, 0xFF0000};

    // int[] shields = new[] {
    //     0xA11511, 0xA11511, 0xA11511, 0xA11511,
    //     0xA11511, 0xA11511, 0xA11511, 0xA11511,
    //     0xA11511, 0xA11511, 0xA11511, 0xA11511};

    // int[] thrust = new[] {
    //     0xEEFF11, 0xEEFF11, 0xEEFF11, 0xEEFF11,
    //     0xEEFF11, 0xEEFF11, 0xEEFF11, 0xEEFF11,
    //     0xEEFF11, 0xEEFF11, 0xEEFF11, 0xEEFF11};

    // int[] thrott = new[] {
    //     0xFF0000, 0xFF0000, 0xFF0000, 0xFF0000,
    //     0xFF0000, 0xFF0000, 0xFF0000, 0xFF0000,
    //     0xFF0000, 0xFF0000, 0xFF0000, 0xFF0000};

    // int[] healths = new[] {
    //     0xFF0000, 0xFF0000, 0xFF0000, 0xCC2200,
    //     0xAA4400, 0x886600, 0x669900, 0x44BB00,
    //     0x22DD00, 0x11FF00, 0x11FF00, 0x11FF00};

    IEnumerator Start() {
        var text = GetComponent<ui::Text>();
        yield return null;
        var spaceship = GetComponentInParent<Spaceship>();
        while (true) {
            yield return new WaitForSeconds(0.1f);
            var health = spaceship.Health/spaceship.MaxHealth;
            var target = spaceship.Target;
            var speed = string.Format("{0:F2} m/s", spaceship.ForwardSpeed);
            var drift = string.Format("{0:F2} m/s",
                spaceship.Velocity.ToVector().magnitude-spaceship.ForwardSpeed);
            var throttle = spaceship.Throttle;
            var boost = spaceship.Energy/spaceship.EnginePower;
            text.text =
                  " ------- -------------- "+
                "\n| F-MODE: "+FormatMode(spaceship.Mode)+" |"+
                "\n|------- --------------|"+
                "\n| SPEED : "+Format(speed)+" |"+
                "\n|------- --------------|"+
                "\n| DRIFT : "+Format(drift)+" |"+
                "\n ------- -------------- ";
        }
    }


    static string ColorHash(int color) {
        return string.Format("<color=#{0:X}>#</color>",color); }


    static string HashBar(float n, int[] colors, int size=12) {
        var s = "";
        var x = Mathf.Min((int) (n*size), size);
        for (var i=0; i<x; ++i) s += ColorHash(colors[Mathf.Min(i,size-1)]);
        return s + new string(' ', Mathf.Min(size,Mathf.Max(0,size-x)));
    }


    string FormatMode(FlightMode mode, int size=12) {
        var s = mode.ToString().Ellipsis(size);
        return s + new string(' ', Mathf.Min(size,Mathf.Max(0,size-s.Length)));
    }

    string Format(string input, int size=12) {
        var s = (string.IsNullOrEmpty(input))?"None":input.Ellipsis(size);
        return s + new string(' ', Mathf.Min(size,Mathf.Max(0,size-s.Length)));
    }
}
