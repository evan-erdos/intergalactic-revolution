
using UnityEngine;
using UnityEngine.PostProcessing;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName="Assets/Profiles/Pilots/NewPilotProfile.asset")]
public class PilotProfile : ScriptableObject {
    public GameObject prefab;
    public string Name = "Evan Erdos";
    public string Nationality = "Frederation®"; // passport / twitter handle
    public int Reputation = 0; // warrants & commendations
    public int Money = 10_000; // credits & loans
    public SpaceshipProfile ship; // spaceships
    public StarProfile star; // stars
}
