/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-07-27 */

using UnityEngine;
using UnityEngine.PostProcessing;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName="Assets/Profiles/Pilots/NewPilotProfile.asset")]
public class PilotProfile : Adventure.Profile<PilotProfile> {
    public GameObject prefab;
    public string Name = "Evan Erdos"; // twitter handle
    public string Nationality = "Frederation®"; // passport
    public int Reputation = 0; // warrants & commendations
    public int Money = 10_000; // credits & loans
    public ShipProfile ship; // spaceships
    public SpobProfile spob; // current location
}
