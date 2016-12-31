using System;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Aeroplane {
    [RequireComponent(typeof(ParticleSystem))]
    public class JetParticleEffect : MonoBehaviour {
        public Color minColour;
        [SerializeField] protected ParticleSystem boostParticles;
        AeroplaneController m_Jet;
        ParticleSystem m_System;
        float m_OriginalStartSize;
        float m_OriginalLifetime;
        Color m_OriginalStartColor;
        float boostLifetime;

        void Start() {
            m_Jet = GetComponentInParent<AeroplaneController>();
            m_System = GetComponent<ParticleSystem>();
            m_OriginalLifetime = m_System.main.startLifetimeMultiplier;
            m_OriginalStartSize = m_System.main.startSizeMultiplier;
            m_OriginalStartColor = m_System.main.startColor.color;
            if (!boostParticles) return;
            var particles = boostParticles.main;
            boostLifetime = boostParticles.main.startLifetimeMultiplier;
            particles.startLifetime = 0;
        }

        void Update() {
            var boosting = m_Jet.Boost?1f:0f;
            var particles = m_System.main;
            particles.startLifetime = Mathf.Lerp(
                0.0f, m_OriginalLifetime, m_Jet.Throttle);
            particles.startSize = Mathf.Lerp(
                m_OriginalStartSize*0.3f+boosting,
                m_OriginalStartSize, m_Jet.Throttle);
            particles.startColor = Color.Lerp(
                minColour, m_OriginalStartColor, m_Jet.Throttle);
            if (!boostParticles) return;
            var boost = boostParticles.main;
            boost.startLifetime = Mathf.Lerp(0f,boostLifetime,boosting);
        }
    }
}
