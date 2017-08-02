/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronautics.Spaceships {
    public class Thruster : Adventure.Object, IShipPart {
        enum LinearAxis { None, Sway, Heave, Surge }
        enum AngularAxis { None, Roll, Pitch, Yaw }
        bool isDisabled, isFlipped, isReverse;
        LinearAxis move = LinearAxis.Sway;
        AngularAxis spin = AngularAxis.Roll;
        float size, life;
        Color color;
        ParticleSystem particles;
        Spaceship ship;

        public void Disable() => isDisabled = true;

        void Start() {
            (ship, particles) = (GetParent<Spaceship>(), GetChild<ParticleSystem>());
            ship.MoveEvent += e => OnMove(e);
            (move, spin, isFlipped, isReverse) = CalculateOrientation();
            life = particles.main.startLifetimeMultiplier;
            size = particles.main.startSizeMultiplier;
            color = particles.main.startColor.color;
            var particleSystem = particles.main;
            particleSystem.startLifetime = 0;
            particles.Play();
        }


        (LinearAxis, AngularAxis, bool, bool) CalculateOrientation(float min=0.2f) {
            // InverseTransformDirection, InverseTransformVector, TransformDirection
            var offset = ship.transform.InverseTransformPoint(transform.position);
            var direction = ship.transform.InverseTransformDirection(transform.forward);
            var (linear, angular, flipped, reverse) = (LinearAxis.None, AngularAxis.None, false, false);
            if ((direction-Vector3.up).magnitude<min) {
                if (min>Mathf.Abs(offset.x)) return (LinearAxis.Heave, AngularAxis.Pitch, false, 0<offset.z);
                else return (LinearAxis.Heave, AngularAxis.Roll, false, 0<offset.x); }
            if ((direction-Vector3.down).magnitude<min) {
                if (min>Mathf.Abs(offset.x)) return (LinearAxis.Heave, AngularAxis.Pitch, true, offset.z<0);
                else return (LinearAxis.Heave, AngularAxis.Roll, true, offset.x<0); }
            if ((direction-Vector3.left).magnitude<min)
                return (LinearAxis.Sway, AngularAxis.Yaw, true, offset.z<0);
            if ((direction-Vector3.right).magnitude<min)
                return (LinearAxis.Sway, AngularAxis.Yaw, false, offset.z>0);
            if ((direction-Vector3.forward).magnitude<min)
                return (LinearAxis.Surge, AngularAxis.None, true, false);
            if ((direction-Vector3.back).magnitude<min)
                return (LinearAxis.Surge, AngularAxis.None, false, true);
            throw new Error($"thruster {name} not aligned");
        }

        // void LateUpdate() => OnMove(); // remove

        void OnMove(FlightArgs e=null) {
            if (isDisabled) { enabled = false; return; }
            var (thrust, particleSystem) = (0f, particles.main);
            switch (spin) {
                case AngularAxis.Roll: thrust = e.Roll*(isReverse?-1:1); break;
                case AngularAxis.Pitch: thrust = e.Pitch*(isReverse?-1:1); break;
                case AngularAxis.Yaw: thrust = e.Yaw*(isReverse?-1:1); break; }

            switch (move) {
                case LinearAxis.Sway: thrust += e.Strafe*(isFlipped?-1:1); break;
                case LinearAxis.Heave: thrust += e.Lift*(isFlipped?-1:1); break;
                case LinearAxis.Surge: thrust += e.Thrust*(isFlipped?-1:1); break; }

            // if (0<thrust && !isReverse || thrust<0 && isReverse) return;
            thrust = Mathf.Max(0,-thrust); // thrust = Mathf.Abs(thrust);
            particleSystem.startLifetime = Mathf.Lerp(0,life,thrust);
            particleSystem.startSize = Mathf.Lerp(size*0.3f,size,thrust);
            particleSystem.startColor = Color.Lerp(Color.black,color,thrust);
        }
    }
}
