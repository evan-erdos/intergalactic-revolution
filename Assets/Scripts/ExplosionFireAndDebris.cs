using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Effects {
    public class ExplosionFireAndDebris : MonoBehaviour {
        public Transform[] debrisPrefabs;
        public Transform firePrefab;
        public int numDebrisPieces = 0;
        public int numFires = 0;

        IEnumerator Start() {
            float multiplier = 1;

            for (int n = 0; n < numDebrisPieces*multiplier; ++n) {
                var prefab = debrisPrefabs[Random.Range(0, debrisPrefabs.Length)];
                var pos = transform.position + Random.insideUnitSphere*3*multiplier;
                var rot = Random.rotation;
                Instantiate(prefab, pos, rot);
            }

            yield return null;
            var r = 10*multiplier;
            var cols = Physics.OverlapSphere(transform.position, r);
            foreach (var col in cols) {
                if (numFires > 0) {
                    RaycastHit fireHit;
                    var fireRay = new Ray(
                        transform.position,
                        col.transform.position - transform.position);
                    if (col.Raycast(fireRay, out fireHit, r)) {
                        AddFire(col.transform, fireHit.point, fireHit.normal);
                        numFires--;
                    }
                }
            }

            var testR = 0f;
            while (numFires > 0 && testR < r) {
                RaycastHit fireHit;
                var fireRay = new Ray(
                    transform.position + Vector3.up,
                    Random.onUnitSphere);
                if (Physics.Raycast(fireRay, out fireHit, testR)) {
                    AddFire(null, fireHit.point, fireHit.normal);
                    numFires--;
                } testR += r*0.1f;
            }
        }


        void AddFire(Transform t, Vector3 pos, Vector3 normal) {
            pos += normal*0.5f;
            var fire = Instantiate(firePrefab,pos,Quaternion.identity) as Transform;
            fire.parent = t;
        }
    }
}
