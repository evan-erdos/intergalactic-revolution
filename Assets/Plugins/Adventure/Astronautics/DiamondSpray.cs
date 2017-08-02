/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Adventure.Astronautics.Spaceships {
    public class DiamondSpray : EnergyBlast {
        [SerializeField] float spread = 100;
        List<Rigidbody> shards = new List<Rigidbody>();

        public override void Reset() { shards.ForEach(o => Reset(o));
            (rigidbody.isKinematic, rigidbody.velocity) = (false, Vector3.zero);

            void Reset(Rigidbody shard) {
                shard.Get<ParticleSystem>().Stop(); shard.Get<TrailRenderer>()?.Clear();
                (shard.isKinematic, shard.transform.parent) = (true, transform);
                (shard.Get<Renderer>().enabled, shard.Get<Collider>().enabled) = (true, true);
                (shard.position, shard.rotation) = (transform.position, transform.rotation);
                (shard.velocity, shard.angularVelocity) = (Vector3.zero, Vector3.zero);
            }
        }

        public override void Fire(Vector3 position, Vector3 velocity, Vector3 initial) {
            Reset(); shards.ForEach(o => Fire(o));

            void Fire(Rigidbody shard) {
                (shard.isKinematic, shard.transform.parent, shard.position) = (false,null,position);
                shard.rotation.SetLookRotation(position-velocity, transform.up);
                shard.AddForce(initial, ForceMode.VelocityChange);
                shard.AddForce(velocity + Random.insideUnitSphere*spread);
            }
        }

        void OnHit(Rigidbody o) => o.Get<ParticleSystem>().Play();

        protected override void Awake() { base.Awake();
            shards.Add(GetChildren<Rigidbody>()); shards.Remove(Get<Rigidbody>());
            shards.ForEach(o => o.Get<EnergyBlast>().HitEvent += e => OnHit(o));
        }
    }
}
