
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName="Assets/Profiles/Spaceships/NewSpaceshipProfile.asset")]
public class SpaceshipProfile : ScriptableObject {
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
    public List<Vector3> Pivots = new List<Vector3> {
        new Vector3(0,0.5f,-0.25f), new Vector3(0,4,-20) };
    public List<AudioClip> hitSounds = new List<AudioClip>();
    public AudioClip modeClip;
    public AudioClip changeClip;
    public AudioClip selectClip;
    public AudioClip hyperspaceClip;
    public AudioClip alarmClip;
    public GameObject explosion;
    public GameObject hyperspace;
}
