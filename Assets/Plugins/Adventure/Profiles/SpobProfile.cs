/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-07-27 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

[CreateAssetMenu(fileName="Assets/Prefabs/Spobs/NewSpobProfile.asset")]
public class SpobProfile : Adventure.Profile<SpobProfile> {
    public string Name = "Station Alpha A";
    public string Faction = "Frederation";
    public StarProfile Star;
    public Vector3 StellarPosition = new Vector3(0,0,0);
}
