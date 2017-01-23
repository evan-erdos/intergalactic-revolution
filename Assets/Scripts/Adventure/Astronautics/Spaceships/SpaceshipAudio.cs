/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Adventure.Astronautics.Spaceships {
    public class SpaceshipAudio : NetworkBehaviour {
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
        [SerializeField] AdvancedSetttings settings = new AdvancedSetttings();

        AudioSource boostSource;
        AudioSource engineSoundSource;
        AudioSource windSoundSource;
        Spaceship spaceship;
        new Rigidbody rigidbody;

        void OnKill() {
            Destroy(engineSoundSource);
            Destroy(windSoundSource);
            Destroy(boostSource);
            enabled = false;
        }


        void Awake() {
            if (SpaceManager.IsOnline && !isLocalPlayer) { enabled = false; return; }
            spaceship = GetComponent<Spaceship>();
            rigidbody = GetComponent<Rigidbody>();
            spaceship.KillEvent += (o,e) => OnKill();

            engineSoundSource = gameObject.AddComponent<AudioSource>();
            engineSoundSource.playOnAwake = false;
            engineSoundSource.clip = m_EngineSound;
            engineSoundSource.minDistance = settings.engineMinDistance;
            engineSoundSource.maxDistance = settings.engineMaxDistance;
            engineSoundSource.loop = true;
            engineSoundSource.dopplerLevel = settings.engineDopplerLevel;

            windSoundSource = gameObject.AddComponent<AudioSource>();
            windSoundSource.playOnAwake = false;
            windSoundSource.clip = m_WindSound;
            windSoundSource.minDistance = settings.windMinDistance;
            windSoundSource.maxDistance = settings.windMaxDistance;
            windSoundSource.loop = true;
            windSoundSource.dopplerLevel = settings.windDopplerLevel;

            boostSource = gameObject.AddComponent<AudioSource>();
            boostSource.playOnAwake = false;
            boostSource.clip = boostSound;
            boostSource.minDistance = settings.engineMinDistance;
            boostSource.maxDistance = settings.engineMaxDistance;
            boostSource.loop = true;
            boostSource.dopplerLevel = settings.engineDopplerLevel;
            boostSource.pitch = 1f;

            Update();

            engineSoundSource.Play();
            windSoundSource.Play();
            boostSource.Play();
        }


        void Update() {
            var enginePowerProportion = Mathf.InverseLerp(
                0, spaceship.EnginePower, spaceship.CurrentPower);

            engineSoundSource.pitch = Mathf.Lerp(
                m_EngineMinThrottlePitch,
                m_EngineMaxThrottlePitch,
                enginePowerProportion);
            engineSoundSource.pitch += spaceship.Energy*m_EngineFwdSpeedMultiplier;
            engineSoundSource.volume = Mathf.InverseLerp(
                0, spaceship.EnginePower*settings.engineMasterVolume,
                spaceship.CurrentPower);

            boostSource.volume = Mathf.Lerp(
                0, settings.engineMasterVolume, spaceship.Boost);

            var speed = rigidbody.velocity.magnitude;
            windSoundSource.pitch = m_WindBasePitch + speed*m_WindSpeedPitchFactor;
            windSoundSource.volume = Mathf.InverseLerp(
                0, m_WindMaxSpeedVolume, speed)*settings.windMasterVolume;
        }
    }
}
