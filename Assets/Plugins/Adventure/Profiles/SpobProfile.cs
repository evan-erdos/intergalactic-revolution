
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName="Assets/Prefabs/Spobs/NewSpobProfile.asset")]
public class SpobProfile : ScriptableObject {
    public GameObject prefab;
    public string Name = "Sol";
    public Vector3 StellarPosition = new Vector3(0,0,0);
    public Scene spob;
}
