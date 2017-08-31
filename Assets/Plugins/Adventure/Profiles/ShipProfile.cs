/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-07-27 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName="Assets/Prefabs/Ships/NewShipProfile.asset")]
public class ShipProfile : Adventure.Profile<ShipProfile> {
    public GameObject prefab;
    public string Name = "T-31 Viper 411";
    public int CargoSpace = 20; // tons
    public float Mass = 15; // tons
    public float Health = 12000; // N
    public float EnginePower = 8000; // kN
    public float RollEffect = 1; // [0..1]
    public float PitchEffect = 1; // [0..1]
    public float YawEffect = 0.2f; // [0..1]
    public float SpinEffect = 1; // [0..1]
    public float ThrottleEffect = 0.5f; // [0..1]
    public float BrakesEffect = 3; // drag coefficient
    public float AeroEffect = 0.02f; // drag coefficient
    public float DragEffect = 0.001f; // drag coefficient
    public float EnergyThrust = 6000; // kN
    public float EnergyCapacity = 4000; // W/L
    public float EnergyLoss = 50; // W/L
    public float EnergyGain = 20; // W/L
    public float TopSpeed = 1500; // m/s
    public float ManeuveringEnergy = 100; // kN
    public float linearEnergy = 800; // kN
    // public Color accentColor = Color.grey; // color
    // public Color energyColor = Color.white; // color
    // public AnimationCurve ThrustResponse = new AnimationCurve(new Keyframe(0,1), new Keyframe(1,1));
    // public AnimationCurve LateralResponse = new AnimationCurve(new Keyframe(0,1), new Keyframe(1,1));
    public List<Vector3> Pivots = new List<Vector3> { new Vector3(0,0.5f,-0.25f) };
    public List<AudioClip> hitSounds = new List<AudioClip>();
    public List<AudioClip> shieldSounds = new List<AudioClip>();
    public GameObject explosion;
    public GameObject hyperspace;
    public GameObject thruster;
    public GameObject motesOfLight;
    [Range(0,1)] public float engineMasterVolume = 0.25f;
    [Range(0,1)] public float windMasterVolume = 0.5f;
    public float engineMinPitch = 0.4f;
    public float engineMaxPitch = 1;
    public float engineForwardFactor = 0.0002f;
    public AudioClip engineClip;
    public AudioClip boostClip;
    public AudioClip windClip;
    public float windPitch = 0.2f;
    public float windPitchSpeed = 0.001f;
    public float windMaxSpeed = 2000;
    public float engineMinDistance = 50;
    public float engineMaxDistance = 1000;
    public float engineDopplerLevel = 1;
    public float windMinDistance = 10;
    public float windMaxDistance = 100;
    public float windDopplerLevel = 1;
    public AudioClip modeClip;
    public AudioClip changeClip;
    public AudioClip selectClip;
    public AudioClip hyperspaceClip;
    public AudioClip alarmClip;
}
