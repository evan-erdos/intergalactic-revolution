/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using Adventure.Astronautics.Spaceships;

namespace Adventure.Astronautics {
    public class VectoredThrust : SpaceObject, IShipComponent, IDamageable {
        Spaceship spaceship;
        [SerializeField] float range = 6; // deg
        [SerializeField] protected bool reverse;
        public float Health {get;protected set;} = 1000;
        public void Disable() => enabled = false;
        public void Damage(float damage) { Health -= damage; if (Health<0) Detonate(); }
        public void Detonate() {
            var rigidbody = GetComponent<Rigidbody>();
            if (!rigidbody) rigidbody = gameObject.AddComponent<Rigidbody>();
            (rigidbody.isKinematic,rigidbody.useGravity) = (false,false);
            transform.parent = null; Disable();
        }

        void Awake() => spaceship = GetComponentInParent<Spaceship>();
        void FixedUpdate() => transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            Quaternion.Euler(
                x: Mathf.Clamp(range*(spaceship.Control.roll
                    * (reverse?1:-1)+spaceship.Control.pitch),5,-5),
                y: spaceship.Control.yaw*range,
                z: spaceship.Control.roll*range*(reverse?0.5f:0.25f)),
            Time.fixedDeltaTime*10);
    }
}
