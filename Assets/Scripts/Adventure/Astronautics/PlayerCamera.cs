/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-12 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;

namespace Adventure.Astronautics {
    public class PlayerCamera : SpaceObject {
        public GameObject spaceCamera;
        public Transform Target {get;set;}
        (Vector3,Quaternion) defaultTarget = (Vector3.zero,Quaternion.identity);

        public static Camera main;
        public static PlayerCamera singleton;

        public static void Follow(Transform target) {
            main.transform.parent = target;
            main.transform.localPosition = Vector3.zero;
            main.transform.localRotation = Quaternion.identity;
        }

        bool PreFollow() => !(Target is null) && Target.IsNear(transform);
        void Awake() => (singleton,main) = (this,Get<Camera>());
        void Start() => Create<SpaceCamera>(spaceCamera).Create(main);

        // public static void Follow(Transform o) => singleton.Target = o;
        // void Follow((Vector3 position, Quaternion rotation) o) =>
        //     (transform.position,transform.rotation) = (o.position,o.rotation);
        // void FixedUpdate() => If(PreFollow, () => Follow(Target));
        // void LateUpdate() => If(PreFollow, () => Follow(Target));
            // if (spaceship) Camera.main.transform.localPosition =
            //     Random.insideUnitCircle * amount *
            //     (Mathf.PerlinNoise(Time.time*amount,perlin)-0.5f) *
            //     Mathf.Clamp(spaceship.Speed/spaceship.TopSpeed-1,0,0.5f);
    }
}
