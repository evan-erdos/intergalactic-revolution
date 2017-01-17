/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using unit=Adventure.Astronautics.Astronomy;

namespace Adventure.Astronautics {
    public class StarSystem : SpaceObject, ICreatable<StarProfile> {
        [SerializeField] protected StarProfile profile;
        GameObject prefab;
        public Vector3 StellarPosition {get;protected set;}
        public StarProfile[] NearbySystems {get;protected set;}
        void Awake() => Create(profile);
        // void Start() => Create(prefab);
        public void Create(StarProfile profile) =>
            (StellarPosition, NearbySystems, prefab) =
                (profile.StellarPosition, profile.NearbySystems, profile.prefab);
    }
}
