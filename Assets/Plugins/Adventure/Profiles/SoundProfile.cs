
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName="Assets/Prefabs/Sounds/NewSoundProfile.asset")]
public class SoundProfile : Adventure.Profile {
    public string Name = "New Sound";
    public float pitch = 1;
    public float volume = 1;
    public float pitchVariance = 0.1f;
    public float volumeVariance = 0.1f;
    public List<AudioClip> sounds = new List<AudioClip>();
}
