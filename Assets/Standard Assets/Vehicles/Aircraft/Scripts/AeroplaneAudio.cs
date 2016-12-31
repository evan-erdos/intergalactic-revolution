using System;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Aeroplane {
    public class AeroplaneAudio : MonoBehaviour {

        [Serializable] public class AdvancedSetttings {
            // The min distance of the engine audio source.
            public float engineMinDistance = 50f;
            // The max distance of the engine audio source.
            public float engineMaxDistance = 1000f;
            // The doppler level of the engine audio source.
            public float engineDopplerLevel = 1f;
            // An overall control of the engine sound volume.
            [Range(0f, 1f)] public float engineMasterVolume = 0.5f;
            // The min distance of the wind audio source.
            public float windMinDistance = 10f;
            // The max distance of the wind audio source.
            public float windMaxDistance = 100f;
            // The doppler level of the wind audio source.
            public float windDopplerLevel = 1f;
            // An overall control of the wind sound volume.
            [Range(0f, 1f)] public float windMasterVolume = 0.5f;
        }

        // pitch and volume are affected by the plane's throttle setting
        [SerializeField] protected AudioClip m_EngineSound;
        [SerializeField] protected AudioClip boostSound;
        // Pitch of the engine sound when at minimum throttle.
        [SerializeField] protected float m_EngineMinThrottlePitch = 0.4f;
        // Pitch of the engine sound when at maximum throttle.
        [SerializeField] protected float m_EngineMaxThrottlePitch = 2f;
        // Additional multiplier for an increase in pitch of the engine
        [SerializeField] protected float m_EngineFwdSpeedMultiplier = 0.002f;
        // pitch and volume are affected by the plane's velocity.
        [SerializeField] protected AudioClip m_WindSound;
        // starting pitch for wind (when plane is at zero speed)
        [SerializeField] protected float m_WindBasePitch = 0.2f;
        // Relative increase in pitch of the wind from the plane's speed.
        [SerializeField] protected float m_WindSpeedPitchFactor = 0.004f;
        // the speed to reach before the wind sound reaches maximum volume.
        [SerializeField] protected float m_WindMaxSpeedVolume = 100;
        // container to make advanced settings appear as rollout in inspector
        [SerializeField] protected AdvancedSetttings settings = new AdvancedSetttings();

        AudioSource boostSource;
        AudioSource m_EngineSoundSource;
        AudioSource m_WindSoundSource;
        AeroplaneController m_Plane;
        Rigidbody m_Rigidbody;

        void Awake() {
            m_Plane = GetComponent<AeroplaneController>();
            m_Rigidbody = GetComponent<Rigidbody>();
            m_EngineSoundSource = gameObject.AddComponent<AudioSource>();
            m_EngineSoundSource.playOnAwake = false;
            m_WindSoundSource = gameObject.AddComponent<AudioSource>();
            m_WindSoundSource.playOnAwake = false;
            boostSource = gameObject.AddComponent<AudioSource>();
            boostSource.playOnAwake = false;

            m_EngineSoundSource.clip = m_EngineSound;
            m_WindSoundSource.clip = m_WindSound;
            boostSource.clip = boostSound;

            m_EngineSoundSource.minDistance = settings.engineMinDistance;
            m_EngineSoundSource.maxDistance = settings.engineMaxDistance;
            m_EngineSoundSource.loop = true;
            m_EngineSoundSource.dopplerLevel = settings.engineDopplerLevel;

            m_WindSoundSource.minDistance = settings.windMinDistance;
            m_WindSoundSource.maxDistance = settings.windMaxDistance;
            m_WindSoundSource.loop = true;
            m_WindSoundSource.dopplerLevel = settings.windDopplerLevel;

            boostSource.minDistance = settings.engineMinDistance;
            boostSource.maxDistance = settings.engineMaxDistance;
            boostSource.loop = true;
            boostSource.dopplerLevel = settings.engineDopplerLevel;

            Update();

            m_EngineSoundSource.Play();
            m_WindSoundSource.Play();
            boostSource.Play();
        }


        void Update() {

            // find what proportion of the engine's power is being used
            var enginePowerProportion = Mathf.InverseLerp(
                0, m_Plane.MaxEnginePower, m_Plane.EnginePower);

            // set the engine's pitch to be proportional to the engine's power
            m_EngineSoundSource.pitch = Mathf.Lerp(
                m_EngineMinThrottlePitch,
                m_EngineMaxThrottlePitch,
                enginePowerProportion);

            // increase pitch by proportional to the forward speed
            m_EngineSoundSource.pitch += m_Plane.ForwardSpeed*m_EngineFwdSpeedMultiplier;

            // set the volume to be proportional to the engine's current power
            m_EngineSoundSource.volume = Mathf.InverseLerp(
                0, m_Plane.MaxEnginePower*settings.engineMasterVolume,
                m_Plane.EnginePower);

            boostSource.volume = m_Plane.Boost?1f:0;

            // set the wind to be proportional to the forward speed
            var planeSpeed = m_Rigidbody.velocity.magnitude;
            m_WindSoundSource.pitch = m_WindBasePitch + planeSpeed*m_WindSpeedPitchFactor;
            m_WindSoundSource.volume = Mathf.InverseLerp(0, m_WindMaxSpeedVolume, planeSpeed)*settings.windMasterVolume;
        }
    }
}
