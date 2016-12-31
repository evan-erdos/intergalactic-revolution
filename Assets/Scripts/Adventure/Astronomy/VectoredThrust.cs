/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Vehicles.Aeroplane;

namespace Adventure.Astronomy.Aeronautics {
    public class VectoredThrust : MonoBehaviour, IShipComponent, IDamageable {
        Spaceship spaceship;
        [SerializeField] float range = 6f;
        [SerializeField] protected bool reverse;
        public float Health {get;protected set;} = 1000;

        public void Disable() { enabled = false; }
        public void Damage(float damage) { Health -= damage; if (Health<0) Detonate(); }
        public void Detonate() {
            var rigidbody = GetComponent<Rigidbody>();
            if (!rigidbody) rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = false;
            rigidbody.useGravity = false;
            transform.parent = null;
            Disable();
        }

        void Awake() { spaceship = GetComponentInParent<Spaceship>(); }

        void FixedUpdate() {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                Quaternion.Euler(
                    x: Mathf.Max(Mathf.Min(
                        spaceship.RollInput*range*(reverse?1:-1)
                        + spaceship.PitchInput*range,5),-5),
                    y: spaceship.YawInput*range,
                    z: spaceship.RollInput*range*(reverse?0.5f:0.25f)),
                Time.fixedDeltaTime*10);
        }
    }
}
