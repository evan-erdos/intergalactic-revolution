/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-08-27 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName="Assets/Resources/GameConfig.asset")]
public class GameProfile : Adventure.Profile<GameProfile> {
    [SerializeField] public PilotProfile pilot;
    [SerializeField] public GameObject prefab;
    [SerializeField] public GameObject menu;
    [SerializeField] public GameObject user;
    [SerializeField] public GameObject cam;
    [SerializeField] public StarProfile[] StarProfiles;
    [SerializeField] public SpobProfile[] SpobProfiles;
    [SerializeField] public PilotProfile[] PilotProfiles;
    [SerializeField] public ShipProfile[] ShipProfiles;
}
