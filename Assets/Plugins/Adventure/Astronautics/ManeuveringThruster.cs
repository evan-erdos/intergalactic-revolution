/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronautics.Spaceships {
    public class ManeuveringThruster : Adventure.Object, IShipComponent {
        bool isDisabled;
        float size, life;
        Color color;
        ParticleSystem particles;
        Spaceship ship;
        [SerializeField] Axis axis = Axis.Roll;
        [SerializeField] protected bool reverse;
        [SerializeField] protected bool landing;

        enum Axis { None, Roll, Pitch, Yaw };

        public void Disable() => isDisabled = true;

        void Start() {
            (ship, particles) = (GetParent<Spaceship>(), GetChild<ParticleSystem>());
            life = particles.main.startLifetimeMultiplier;
            size = particles.main.startSizeMultiplier;
            color = particles.main.startColor.color;
            var particleSystem = particles.main;
            particleSystem.startLifetime = 0;
            particles.Play();
        }

        void Update() {
            if (isDisabled) return;
            var (thrust, particleSystem) = (0f, particles.main);
            switch (axis) {
                case Axis.Roll: thrust = ship.Control.roll; break;
                case Axis.Pitch: thrust = ship.Control.pitch; break;
                case Axis.Yaw: thrust = ship.Control.yaw; break; }
            if (0<thrust && !reverse || thrust<0 && reverse) return;
            thrust = Mathf.Abs(thrust);
            particleSystem.startLifetime = Mathf.Lerp(0,life,thrust);
            particleSystem.startSize = Mathf.Lerp(size*0.3f,size,thrust);
            particleSystem.startColor = Color.Lerp(Color.black,color,thrust);
        }
    }
}
