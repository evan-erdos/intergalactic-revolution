/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronomy.Aeronautics {
    public class SpaceshipAudio : MonoBehaviour {
        [Serializable] public class AdvancedSetttings {
            public float engineMinDistance = 50f;
            public float engineMaxDistance = 1000f;
            public float engineDopplerLevel = 1f;
            [Range(0f, 1f)] public float engineMasterVolume = 0.5f;
            public float windMinDistance = 10f;
            public float windMaxDistance = 100f;
            public float windDopplerLevel = 1f;
            [Range(0f, 1f)] public float windMasterVolume = 0.5f;
        }

        [SerializeField] protected AudioClip m_EngineSound;
        [SerializeField] protected AudioClip boostSound;
        [SerializeField] protected float m_EngineMinThrottlePitch = 0.4f;
        [SerializeField] protected float m_EngineMaxThrottlePitch = 2f;
        [SerializeField] protected float m_EngineFwdSpeedMultiplier = 0.002f;
        [SerializeField] protected AudioClip m_WindSound;
        [SerializeField] protected float m_WindBasePitch = 0.2f;
        [SerializeField] protected float m_WindSpeedPitchFactor = 0.004f;
        [SerializeField] protected float m_WindMaxSpeedVolume = 100;
        [SerializeField] protected AdvancedSetttings settings = new AdvancedSetttings();

        AudioSource boostSource;
        AudioSource m_EngineSoundSource;
        AudioSource m_WindSoundSource;
        Spaceship m_Plane;
        Rigidbody m_Rigidbody;

        void Awake() {
            m_Plane = GetComponent<Spaceship>();
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
