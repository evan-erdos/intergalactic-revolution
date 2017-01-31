
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName="Assets/Profiles/Sounds/NewSoundProfile.asset")]
public class SoundProfile : ScriptableObject {
    public GameObject prefab;
    public string Name = "New Sound";
    public float pitch = 1;
    public float volume = 1;
    public float pitchVariance = 0.1f;
    public float volumeVariance = 0.1f;
    public AudioClip[] sounds;
}
