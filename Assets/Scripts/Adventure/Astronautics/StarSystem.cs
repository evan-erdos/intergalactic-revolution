/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using unit=Adventure.Astronautics.Astronomy;

namespace Adventure.Astronautics {
    public class StarSystem : SpaceObject, ICreatable<StarSystemProfile> {
        [SerializeField] protected StarSystemProfile profile;
        GameObject starSystem;
        public Vector3 StellarPosition {get;protected set;}
        public List<StarSystemProfile> NearbySystems {get;protected set;}
        void Awake() => Create(profile);
        void Start() => Create(starSystem);
        public void Create(StarSystemProfile profile) =>
            (StellarPosition, NearbySystems, starSystem) =
                (profile.StellarPosition, profile.NearbySystems, profile.starSystem);
    }
}
