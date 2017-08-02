/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-07-27 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName="Assets/Prefabs/Ships/NewShipProfile.asset")]
public class ShipProfile : Adventure.Profile {
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
    // public AnimationCurve ThrustResponse = new AnimationCurve(new Keyframe(0,1), new Keyframe(1,1));
    // public AnimationCurve LateralResponse = new AnimationCurve(new Keyframe(0,1), new Keyframe(1,1));
    public List<Vector3> Pivots = new List<Vector3> { new Vector3(0,0.5f,-0.25f) };
    public List<AudioClip> hitSounds = new List<AudioClip>();
    public AudioClip modeClip;
    public AudioClip changeClip;
    public AudioClip selectClip;
    public AudioClip hyperspaceClip;
    public AudioClip alarmClip;
    public GameObject explosion;
    public GameObject hyperspace;
}
