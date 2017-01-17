
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName="Assets/Profiles/Weapons/NewBlasterProfile.asset")]
public class BlasterProfile : ScriptableObject {
    public GameObject prefab;
    public string Name = "High Energy Blaster";
    public float Health = 1000; // N
    public float Force = 4000; // N
    public float Rate = 10; // Hz
    public float Spread = 100; // m
    public float Range = 1000; // m
    public float Angle = 60; // deg
    public GameObject Projectile;
    public List<AudioClip> sounds;
}
