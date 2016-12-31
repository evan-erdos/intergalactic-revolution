using System;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Aeroplane {
    public class LandingGear : MonoBehaviour {
        private enum GearState { Raised = -1, Lowered = 1 }
        public float raiseAtAltitude = 40;
        public float lowerAtAltitude = 40;
        GearState state = GearState.Lowered;
        Animator animator;
        new Rigidbody rigidbody;

        void Awake() {
            animator = GetComponent<Animator>();
            rigidbody = GetComponent<Rigidbody>();
        }

        void Update() {
            if (state == GearState.Lowered && rigidbody.velocity.y > 0)
                state = GearState.Raised;
            if (state == GearState.Raised && rigidbody.velocity.y < 0)
                state = GearState.Lowered;
            animator.SetInteger("GearState", (int) state);
        }
    }
}
