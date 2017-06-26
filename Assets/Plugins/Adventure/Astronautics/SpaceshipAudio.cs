/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Adventure.Astronautics.Spaceships {
    public class SpaceshipAudio : NetObject {

        AudioSource boostAudio, engineAudio, windAudio;
        Spaceship spaceship;
        new Rigidbody rigidbody;

        [SerializeField] protected AudioClip m_EngineSound;
        [SerializeField] protected AudioClip boostSound;
        [SerializeField] protected float m_EngineMinThrottlePitch = 0.4f;
        [SerializeField] protected float m_EngineMaxThrottlePitch = 2;
        [SerializeField] protected float m_EngineFwdSpeedMultiplier = 0.002f;
        [SerializeField] protected AudioClip m_WindSound;
        [SerializeField] protected float m_WindBasePitch = 0.2f;
        [SerializeField] protected float m_WindSpeedPitchFactor = 0.004f;
        [SerializeField] protected float m_WindMaxSpeedVolume = 100;
        [SerializeField] AdvancedSetttings settings = new AdvancedSetttings();

        [Serializable] public class AdvancedSetttings {
            public float engineMinDistance = 50;
            public float engineMaxDistance = 1000;
            public float engineDopplerLevel = 1;
            [Range(0,1)] public float engineMasterVolume = 0.5f;
            public float windMinDistance = 10;
            public float windMaxDistance = 100;
            public float windDopplerLevel = 1;
            [Range(0,1)] public float windMasterVolume = 0.5f;
        }

        void OnKill() {
            Destroy(engineAudio); Destroy(windAudio); Destroy(boostAudio);
            enabled = false;
        }


        void Awake() {
            if (Manager.IsOnline && !isLocalPlayer) { enabled = false; return; }
            (rigidbody, spaceship) = (Get<Rigidbody>(), Get<Spaceship>());
            spaceship.KillEvent += (o,e) => OnKill();

            engineAudio = gameObject.AddComponent<AudioSource>();
            engineAudio.playOnAwake = false;
            engineAudio.clip = m_EngineSound;
            engineAudio.minDistance = settings.engineMinDistance;
            engineAudio.maxDistance = settings.engineMaxDistance;
            engineAudio.loop = true;
            engineAudio.dopplerLevel = settings.engineDopplerLevel;

            windAudio = gameObject.AddComponent<AudioSource>();
            windAudio.playOnAwake = false;
            windAudio.clip = m_WindSound;
            windAudio.minDistance = settings.windMinDistance;
            windAudio.maxDistance = settings.windMaxDistance;
            windAudio.loop = true;
            windAudio.dopplerLevel = settings.windDopplerLevel;

            boostAudio = gameObject.AddComponent<AudioSource>();
            boostAudio.playOnAwake = false;
            boostAudio.clip = boostSound;
            boostAudio.minDistance = settings.engineMinDistance;
            boostAudio.maxDistance = settings.engineMaxDistance;
            boostAudio.loop = true;
            boostAudio.dopplerLevel = settings.engineDopplerLevel;
            boostAudio.pitch = 1;

            Update();

            engineAudio.Play();
            windAudio.Play();
            boostAudio.Play();
        }


        void Update() {
            var engineProportion = Mathf.InverseLerp(
                0,spaceship.EnginePower,spaceship.CurrentPower);
            engineAudio.pitch = Mathf.Lerp(
                m_EngineMinThrottlePitch,m_EngineMaxThrottlePitch,engineProportion);
            engineAudio.pitch += spaceship.Energy*m_EngineFwdSpeedMultiplier;
            engineAudio.volume = Mathf.InverseLerp(
                0, spaceship.EnginePower*settings.engineMasterVolume,
                spaceship.CurrentPower);
            boostAudio.volume = Mathf.Lerp(0,settings.engineMasterVolume,spaceship.Boost);
            var speed = rigidbody.velocity.magnitude;
            windAudio.pitch = m_WindBasePitch + speed*m_WindSpeedPitchFactor;
            windAudio.volume = Mathf.InverseLerp(
                0,m_WindMaxSpeedVolume,speed)*settings.windMasterVolume;
        }
    }
}
