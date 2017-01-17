/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Cameras;
using Adventure.Astronautics.Spaceships;

namespace Adventure.Astronautics {
    public abstract class SpaceActor : NetworkSpaceObject {
        new protected Rigidbody rigidbody;
        protected List<NetworkStartPosition> locations =
            new List<NetworkStartPosition>();
        public virtual Spaceship Ship {get;set;}
        public (StarProfile star,string subsystem) Destination {get;set;}

        public virtual void SetShip(Spaceship ship) =>
            (Ship, PlayerCamera.Target) = (ship, ship.transform);

        protected virtual void Awake() {
            rigidbody = GetOrAdd<Rigidbody>();
            locations.Add(FindObjectsOfType<NetworkStartPosition>());
            DontDestroyOnLoad(gameObject);
        }

        protected virtual void Reset() {
            var point = locations.Pick();
            transform.position = point.transform.position;
            transform.rotation = point.transform.rotation;
        }

        protected virtual void FixedUpdate() {
            if (Ship is null) return;
            transform.position = Ship.transform.position;
            transform.rotation = Ship.transform.rotation;
        }
    }
}
