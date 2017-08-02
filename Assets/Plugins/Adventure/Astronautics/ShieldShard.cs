/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-07-30 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronautics.Spaceships {
    public class ShieldShard : Adventure.Object, IDamageable {
        new Collider collider;
        new Renderer renderer;
        public bool IsAlive => 0<Health;
        public float Health {get;protected set;} = 2000;
        public float MaxHealth {get;protected set;} = 2000;
        public void Reset() => (collider.enabled, renderer.enabled, Health) = (true,true,MaxHealth);
        public void Kill() => (collider.enabled, renderer.enabled) = (false,false);
        public void Hit(float damage=0) { if ((Health-=damage)<0) Kill(); }
        void Awake() => (collider, renderer) = (Get<Collider>(), Get<Renderer>());
        void OnCollisionEnter(Collision c) => Hit(c.impulse.magnitude/4);
    }
}
