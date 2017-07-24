/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Adventure.Astronautics.Spaceships {
    public class DiamondSpray : EnergyBlast {
        [SerializeField] float spread = 100;
        [SerializeField] float power = 30000;
        List<Rigidbody> shards = new List<Rigidbody>();

        void OnHit(Rigidbody o) => o.Get<ParticleSystem>().Play();
        public override void Reset() { shards.ForEach(o => Reset(o));
            (rigidbody.isKinematic, rigidbody.velocity) = (false, Vector3.zero); }

        protected override void Awake() { base.Awake();
            shards.Add(GetComponentsInChildren<Rigidbody>());
            shards.Remove(Get<Rigidbody>());
            shards.ForEach(o => o.Get<EnergyBlast>().HitEvent += (e,a) => OnHit(o));
        }

        void Start() => shards.ForEach(o => Fire(o));

        void Reset(Rigidbody shard) {
            shard.Get<ParticleSystem>().Stop();
            shard.transform.parent = transform;
            (shard.Get<Renderer>().enabled, shard.Get<Collider>().enabled) = (true, true);
            (shard.isKinematic, shard.velocity) = (false, Vector3.zero);
        }

        void Fire(Rigidbody shard) {
            (shard.isKinematic, shard.velocity) = (false, rigidbody.velocity);
            shard.angularVelocity = rigidbody.angularVelocity;
            shard.angularVelocity += Random.insideUnitSphere*spread;
            shard.velocity += Random.insideUnitSphere*spread;
            shard.AddForce(transform.forward*power);
            shard.transform.parent = null;
        }
    }
}
