/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using unit=Adventure.Astronautics.Astronomy;

namespace Adventure.Astronautics {
    public class StarSystem : SpaceObject {
        string path = "Scenes/Star Systems/";
        public string Name {get;protected set;} = "Epsilon Eridani";
        public Vector3 StellarPosition {get;protected set;} = Vector3.zero;
        public Map<StarSystem> Systems {get;protected set;} = new Map<StarSystem>();
        public void Awake() => Load();
        void Load() => SceneManager.LoadScene($"{path}{name}", LoadSceneMode.Additive);

        public new class Data : SpaceObject.Data {
            public List<string> systems {get;set;}
            public double[] position {get;set;}
            public override SpaceObject Deserialize(SpaceObject o) {
                var instance = base.Deserialize(o) as StarSystem;
                instance.StellarPosition = new Vector3(
                    x: (float) position[0],
                    y: (float) position[1],
                    z: (float) position[2]);
                foreach (var system in systems)
                    instance.Systems[system] = SpaceManager.StarSystems[system];
                return instance;
            }
        }
    }
}
