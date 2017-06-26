using System;
using System.Collections;
using UnityEngine;

namespace UnityStandardAssets.Effects {
    public class Explosive : MonoBehaviour {
        public Transform explosionPrefab;
        public float detonationImpactVelocity = 10;
        public float sizeMultiplier = 1;
        public bool reset = true;
        public float resetTimeDelay = 10;
        private bool m_Exploded;
        // private ObjectResetter m_ObjectResetter;

        // void Start() => m_ObjectResetter = GetComponent<ObjectResetter>();
        public void Reset() => m_Exploded = false;

        IEnumerator OnCollisionEnter(Collision col) {
            if (!enabled || col.contacts.Length<=0 || m_Exploded) yield break;
            if (Vector3.Project(col.relativeVelocity,col.contacts[0].normal).magnitude > detonationImpactVelocity) {
                Instantiate(explosionPrefab, col.contacts[0].point, Quaternion.LookRotation(col.contacts[0].normal));
                m_Exploded = true; SendMessage("Immobilize");
                // if (reset) m_ObjectResetter.DelayedReset(resetTimeDelay);
            } yield return null;
        }
    }
}
