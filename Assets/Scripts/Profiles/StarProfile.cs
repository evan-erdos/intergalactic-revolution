
using UnityEngine;
using UnityEngine.PostProcessing;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName="Assets/Profiles/StarSystems/NewStarProfile.asset")]
public class StarProfile : ScriptableObject {
    public GameObject prefab;
    public string Name = "Sol";
    public Vector3 StellarPosition = new Vector3(0,0,0);
    public PostProcessingProfile atmosphere;
    public string[] Subsystems = new[] {"Moon Base Delta"};
    public StarProfile[] NearbySystems = new StarProfile[1];
}
