/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronautics.Spaceships {
    public class ManeuveringThruster : SpaceObject, IShipComponent {
        bool isDisabled;
        float size, lifetime;
        Color color, minColour = Color.black;
        ParticleSystem particles;
        Spaceship spaceship;
        [SerializeField] Axis axis = Axis.Roll;
        [SerializeField] protected bool reverse;
        [SerializeField] protected bool landing;

        enum Axis { None, Roll, Pitch, Yaw };

        public void Disable() => isDisabled = true;

        void Start() {
            spaceship = GetComponentInParent<Spaceship>();
            particles = GetComponentInChildren<ParticleSystem>();
            lifetime = particles.main.startLifetimeMultiplier;
            size = particles.main.startSizeMultiplier;
            color = particles.main.startColor.color;
            var particleSystem = particles.main;
            particleSystem.startLifetime = 0;
            particles.Play();
        }

        void Update() {
            if (isDisabled) return;
            var (throttle, particleSystem) = (0f, particles.main);
            switch (axis) {
                case Axis.Roll: throttle = spaceship.Control.roll; break;
                case Axis.Pitch: throttle = spaceship.Control.pitch; break;
                case Axis.Yaw: throttle = spaceship.Control.yaw; break; }
            if (0<throttle && !reverse || throttle<0 && reverse) return;
            throttle = Mathf.Abs(throttle);
            particleSystem.startLifetime = Mathf.Lerp(0,lifetime,throttle);
            particleSystem.startSize = Mathf.Lerp(size*0.3f,size,throttle);
            particleSystem.startColor = Color.Lerp(minColour,color,throttle);
        }
    }
}
