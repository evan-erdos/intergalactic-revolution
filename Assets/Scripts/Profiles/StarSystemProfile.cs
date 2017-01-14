
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName="Assets/NewStarSystemProfile.asset")]
public class StarSystemProfile : ScriptableObject {
    public string Name = "Sol";
    public Vector3 StellarPosition = new Vector3(0,0,0);
    public GameObject starSystem;
    public List<StarSystemProfile> NearbySystems = new List<StarSystemProfile>();
}
