/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using unit=Adventure.Astronautics.Astronomy;

namespace Adventure.Astronautics {
    public class StarSystem : SpaceObject {
        string path = "Scenes/Star Systems/";
        public Vector3 StellarPosition {get;protected set;} = Vector3.zero;
        public Map<StarSystem> Systems {get;protected set;} = new Map<StarSystem>();
        public void Awake() => Load();
        void Load() => SceneManager.LoadScene($"{path}{name}", LoadSceneMode.Additive);
    }
}
