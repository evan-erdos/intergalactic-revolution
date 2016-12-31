/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronomy.Aeronautics {
    public class ManeuveringThruster : SpaceObject {
        bool disabled;
        float size, lifetime;
        Color color, minColour = Color.black;
        ParticleSystem particles;
        Spaceship spaceship;
        [SerializeField] Axis axis = Axis.Roll;
        [SerializeField] protected bool reverse;
        [SerializeField] protected bool landing;

        enum Axis { None, Roll, Pitch, Yaw };

        public void Disable() { disabled = true; }

        void Start() {
            spaceship = GetComponentInParent<Spaceship>();
            particles = GetComponentInChildren<ParticleSystem>();
            lifetime = particles.main.startLifetimeMultiplier;
            size = particles.main.startSizeMultiplier;
            color = particles.main.startColor.color;
            var particleSystem = particles.main;
            particleSystem.startLifetime = 0;
        }

        void Update() {
            if (disabled) return;
            var throttle = 0f;
            var particleSystem = particles.main;

            switch (axis) {
                case Axis.Roll: throttle = spaceship.RollInput; break;
                case Axis.Pitch: throttle = spaceship.PitchInput; break;
                case Axis.Yaw: throttle = spaceship.YawInput; break;
            }

            if (0<throttle && !reverse || throttle<0 && reverse) return;
            throttle = Mathf.Abs(throttle);
            particleSystem.startLifetime = Mathf.Lerp(0,lifetime,throttle);
            particleSystem.startSize = Mathf.Lerp(size*0.3f,size,throttle);
            particleSystem.startColor = Color.Lerp(minColour,color,throttle);
        }
    }
}
