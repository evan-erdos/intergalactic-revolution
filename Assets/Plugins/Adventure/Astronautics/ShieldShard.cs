/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-07-30 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronautics.Spaceships {
    public class ShieldShard : Adventure.Object, IShield {
        protected Animator animator;
        protected new Collider collider;
        protected new Renderer renderer;
        public bool IsAlive => 0<Health;
        public float Energy {get;protected set;} = 400;
        public float Health {get;protected set;} = 2000;
        public float MaxHealth => 2000;
        public void Reset() => (animator.enabled, collider.enabled, Health) = (true,true,MaxHealth);
        public void Kill() => (animator.enabled, collider.enabled, renderer.enabled) = (false,false,false);
        public void Hit(float damage=0) => OnHit(new CombatArgs { Sender=this, Damage=damage });
        void Awake() => (animator, collider, renderer) = (Get<Animator>(), Get<Collider>(), Get<Renderer>());
        void OnCollisionEnter(Collision c) => Hit(c.impulse.magnitude/4f);
        void OnHit(CombatArgs e) { if ((Health-=e.Damage)<0) Kill(); animator?.SetTrigger("Hit"); }
    }
}
