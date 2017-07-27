/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-04 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using ai=UnityEngine.AI;
using Adventure.Locales;
using Adventure.Inventories;
using Adventure.Statistics;
using Adventure.Movement;

namespace Adventure {
    public class Person : Actor {
        List<Rigidbody> rigidbodies = new List<Rigidbody>();
        [SerializeField] protected Transform hand;
        protected AvatarIKGoal handGoal = AvatarIKGoal.RightHand;
        public bool IKEnabled {get;set;}
        public uint massLimit {get;set;}
        public Lamp EquippedItem {get;set;}
        public ai::NavMeshAgent agent {get;protected set;}
        public IMotor motor {get;protected set;}
        public Animator animator {get;protected set;}
        public override void Use(IUsable o) => Do(o.Position, () => o.Use());
        public override void Sit(IThing o) => Do(o.Position, () => OnSit());
        public override void Find(IThing o) => Do(o.Position, () => o.Find());
        public override void Push(IPushable o) => Do(o.Position, () => o.Push());
        public override void Pull(IPushable o) => Do(o.Position, () => o.Pull());
        public override void Open(IOpenable o) => Do(o.Position, () => o.Open());
        public override void Shut(IOpenable o) => Do(o.Position, () => o.Shut());
        public override void Read(IReadable o) => Do(o.Position, () => o.Read());
        public override void Wear(IWearable o) => Do(o.Position, () => o.Wear());
        public override void Stow(IWearable o) => Do(o.Position, () => o.Stow());
        public void Do(Transform o, Action e) => Do(o.position, e);
        public void Do(Vector3 o, Action e) => Do(() => transform.IsNear(o),e);
        public void Kill(Person o) => Do(o.transform, () => o.Kill(1000));
        public void Kill(float force=0) => Kill(force*Vector3.one,Vector3.right);

        public override void Goto(StoryArgs e=null) {
            try {
                var query =
                    from thing in Story.Rooms.Values
                    where thing.Fits(e.Input) && thing is Room select thing;
                if (!query.Any()) throw new StoryError(Description["cannot nearby room"]);
                if (query.Count()>1) throw new AmbiguityError(
                    Description?["many nearby room"], query.Cast<IThing>());
                e.Goal = query.First();
                if (e.Goal is Thing location) Goto(location);
                else throw new StoryError($"You can't go to the {e.Goal}.");
            } catch (StoryError) { }
        }

        protected override void Awake() { base.Awake();
            (motor, agent) = (GetChild<IMotor>(), GetChild<ai::NavMeshAgent>());
            (animator, rigidbodies) = (GetChild<Animator>(), GetChildren<Rigidbody>());
            if (agent) (agent.updateRotation, agent.updatePosition) = (false, true);
            rigidbodies.ForEach(o => o.isKinematic = true);
            rigidbodies.Remove(Get<Rigidbody>()); Get<Rigidbody>().isKinematic = true;
        }

        IEnumerator Start() {
            var (delay,threshold,radius,lastPosition) = (1f,1f,0.5f,transform.position);
            while (true) {
                yield return new WaitForSeconds(delay);
                var position = WalkTarget? WalkTarget.position : transform.position;
                if (threshold>(position-lastPosition).sqrMagnitude) continue;
                var v = UnityEngine.Random.insideUnitCircle.normalized*radius;
                WalkTarget.position = position+new Vector3(v.x,0,v.y);
                lastPosition = position;
            }
        }

        void FixedUpdate() {
            if (hand && EquippedItem.transform is Transform t)
                (t.position, t.rotation) = (hand.position, hand.rotation);
            if (!agent || !agent.enabled) return;
            agent.SetDestination(WalkTarget.position);
            motor?.Move(
                move: (agent.remainingDistance>agent.stoppingDistance)
                    ? agent.desiredVelocity : Vector3.zero,
                duck: false,
                jump: false);
        }

        void OnAnimatorIK(int layerIndex) {
            if (!animator || !IKEnabled) return;
            var weight = (EquippedItem==null)?0:1;
            animator.SetLookAtWeight(0);
            animator.SetIKPositionWeight(handGoal,weight);
            animator.SetIKRotationWeight(handGoal,weight);
            animator.SetIKPosition(handGoal,EquippedItem.Grip.position);
            animator.SetIKRotation(handGoal,EquippedItem.Grip.rotation);
            if (LookTarget is null) return;
            animator.SetLookAtWeight(0.5f);
            animator.SetLookAtPosition(LookTarget.position);
        }

        void OnCollisionEnter(Collision o) => Kill(o.impulse/Time.fixedDeltaTime, o.contacts.FirstOrDefault().point);

        public void Kill(Vector3 force, Vector3 position) {
            if (force.sqrMagnitude<100) return;
            if (Get<ai::NavMeshAgent>() is ai::NavMeshAgent n) n.enabled = false;
            if (Get<Animator>() is Animator a) a.enabled = false;
            if (Get<Collider>() is Collider c) c.enabled = false;
            rigidbodies.ForEach(rb => rb.isKinematic = false);
            Get<Rigidbody>().isKinematic = true;
            if (force.sqrMagnitude<=100) return;
            Get<Person>().Kill();
            rigidbodies.ForEach(o => o.AddForceAtPosition(force,position));
        }


        public virtual void View(Transform location) {
            StartSemaphore(Viewing);
            IEnumerator Viewing() {
                var speed = Vector3.zero;
                while (LookTarget.IsNear(location,2)) yield return Wait(() =>
                    LookTarget.position = Vector3.SmoothDamp(
                        current: LookTarget.position, target: location.position,
                        currentVelocity: ref speed, smoothTime: 1));
                yield return new WaitForSeconds(3);
            }
        }

        public void TravelTo(Transform location) {
            motor.IsDisabled = true;
            if (agent) agent.enabled = false;
            transform.position = location.position;
            if (Physics.Raycast(
                origin: transform.position, direction: Vector3.down,
                hitInfo: out RaycastHit hit, maxDistance: 1)) transform.position = hit.point;
            WalkTarget = location;
            if (agent) agent.enabled = true;
            motor.IsDisabled = false;
            motor.Get<Rigidbody>().AddForce(Physics.gravity);
        }


        public void Do(Cond cond, Action then) {
            StartSemaphore(Doing);
            IEnumerator Doing() {
                yield return new WaitWhile(() => cond());
                yield return new WaitForSeconds(0.5f); then();
            }
        }

        void OnSit() { Log(Description["sit"]); animator?.SetBool("Sit",true); }

        public override void Stand() { base.Stand();
            Log(Description["stand"]); animator?.SetBool("Sit",false); }

        public virtual void Stand(StoryArgs e=null) {
            if (!(e.Sender is Actor actor)) throw new StoryError($"The {e.Sender} can't stand.");
            if (animator.GetBool("Sit")) actor.Stand();
            else throw new StoryError(actor["try to stand"]); }

        public override void Pray() { base.Pray(); animator?.SetBool("Pray", true); }

        new public class Data : Actor.Data {
            public override Object Deserialize(Object o) {
                var instance = base.Deserialize(o) as Person;
                return instance;
            }
        }
    }
}
