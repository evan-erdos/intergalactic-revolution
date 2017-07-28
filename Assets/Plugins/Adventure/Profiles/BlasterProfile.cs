/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-07-27 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName="Assets/Prefabs/Items/NewBlasterProfile.asset")]
public class BlasterProfile : Adventure.Profile {
    public GameObject prefab;
    public string Name = "Energy Blaster";
    public float Health = 1000; // N
    public float Force = 4000; // N
    public float Rate = 10; // Hz
    public float Spread = 100; // m
    public float Range = 1000; // m
    public float Angle = 60; // deg
    public GameObject Projectile;
    public SoundProfile sound;
}
