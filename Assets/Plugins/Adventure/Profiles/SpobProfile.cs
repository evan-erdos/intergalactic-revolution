using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

[CreateAssetMenu(fileName="Assets/Prefabs/Spobs/NewSpobProfile.asset")]
public class SpobProfile : Adventure.Profile {
    public string Name = "Station Alpha A";
    public StarProfile Star;
    public string Faction = "Frederation";
    public Vector3 StellarPosition = new Vector3(0,0,0);
}
