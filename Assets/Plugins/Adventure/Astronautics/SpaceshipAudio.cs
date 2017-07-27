/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Adventure.Astronautics.Spaceships {
    public class SpaceshipAudio : NetObject {
        AudioSource thrust, engine, wind;
        Spaceship ship;

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
            [Range(0,1)] public float windMasterVolume = 0.5f; }

        void OnKill() { Delete(engine, wind, thrust); enabled = false; }

        void Awake() {
            if (Manager.IsOnline && !isLocalPlayer) { enabled = false; return; }

            ship = Get<Spaceship>();
            ship.KillEvent += e => OnKill();
            engine = gameObject.AddComponent<AudioSource>();
            wind = gameObject.AddComponent<AudioSource>();
            thrust = gameObject.AddComponent<AudioSource>();

            (engine.playOnAwake, engine.loop) = (false,true);
            engine.minDistance = settings.engineMinDistance;
            engine.maxDistance = settings.engineMaxDistance;
            engine.dopplerLevel = settings.engineDopplerLevel;

            (wind.playOnAwake, wind.loop) = (false,true);
            wind.clip = m_WindSound;
            wind.minDistance = settings.windMinDistance;
            wind.maxDistance = settings.windMaxDistance;
            wind.dopplerLevel = settings.windDopplerLevel;

            (thrust.playOnAwake, thrust.loop) = (false,true);
            thrust.clip = boostSound;
            thrust.minDistance = settings.engineMinDistance;
            thrust.maxDistance = settings.engineMaxDistance;
            thrust.dopplerLevel = settings.engineDopplerLevel;
            thrust.pitch = 1;
            Update();
            engine.Play(); wind.Play(); thrust.Play();
        }


        void Update() {
            var engineProportion = Mathf.InverseLerp(0,ship.EnginePower,ship.CurrentPower);
            engine.pitch = Mathf.Lerp(m_EngineMinThrottlePitch,m_EngineMaxThrottlePitch,engineProportion);
            engine.pitch += ship.Energy*m_EngineFwdSpeedMultiplier;
            engine.volume = Mathf.InverseLerp(0, ship.EnginePower*settings.engineMasterVolume, ship.CurrentPower);
            thrust.volume = Mathf.Lerp(0,settings.engineMasterVolume,ship.Thrust);
            wind.pitch = m_WindBasePitch + ship.Speed*m_WindSpeedPitchFactor;
            wind.volume = Mathf.InverseLerp(0,m_WindMaxSpeedVolume,ship.Speed)*settings.windMasterVolume;
        }
    }
}
