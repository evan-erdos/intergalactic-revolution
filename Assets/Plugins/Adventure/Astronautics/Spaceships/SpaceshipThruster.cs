/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using UnityEngine;

namespace Adventure.Astronautics.Spaceships {
    [RequireComponent(typeof(ParticleSystem))]
    public class SpaceshipThruster : MonoBehaviour {
        Color color, baseColor = Color.black;
        float size, lifetime, boost;
        ParticleSystem particles;
        Spaceship spaceship;
        [SerializeField] protected ParticleSystem boostParticles;

        void Start() {
            spaceship = GetComponentInParent<Spaceship>();
            particles = GetComponent<ParticleSystem>();
            lifetime = particles.main.startLifetimeMultiplier;
            size = particles.main.startSizeMultiplier;
            color = particles.main.startColor.color;
            if (!boostParticles) return;
            var boosts = boostParticles.main;
            boost = boostParticles.main.startLifetimeMultiplier;
            boosts.startLifetime = 0;
            particles.Play();
        }

        void Update() {
            var particleSystem = particles.main;
            particleSystem.startLifetime = Mathf.Lerp(
                0.0f, lifetime, spaceship.Throttle);
            particleSystem.startSize = Mathf.Lerp(
                size*0.3f+spaceship.Boost,
                size, spaceship.Throttle);
            particleSystem.startColor = Color.Lerp(
                color,color,spaceship.Throttle);
            if (!boostParticles) return;
            var boosts = boostParticles.main;
            boosts.startLifetime = Mathf.Lerp(0f,boost,spaceship.Boost);
        }
    }
}
