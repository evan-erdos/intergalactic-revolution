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

        public override void Goto(Thing o, StoryArgs e) {
            try {
                var query =
                    from thing in Story.Rooms.Values
                    where thing.Fits(e.Input) && thing is Room
                    select thing;
                if (!query.Any())
                    throw new StoryError(Description["cannot nearby room"]);
                if (query.Count()>1)
                    throw new AmbiguityError(
                        Description?["many nearby room"],
                        query.Cast<IThing>());
                e.Goal = query.First();
                if (e.Goal is Thing location) Goto(location);
                else throw new StoryError($"You can't go to the {e.Goal}.");
            } catch (StoryError) { }
        }

        protected override void Awake() { base.Awake();
            motor = GetComponentInChildren<IMotor>();
            animator = GetComponentInChildren<Animator>();
            rigidbodies.AddRange(GetComponentsInChildren<Rigidbody>());
            rigidbodies.ForEach(rb => rb.isKinematic = true);
            rigidbodies.Remove(GetComponent<Rigidbody>());
            GetComponent<Rigidbody>().isKinematic = true;
            agent = GetComponentInChildren<ai::NavMeshAgent>();
            // hand = FindOrAdd("hand"); // handGoal = AvatarIKGoal.LeftHand;
            if (!agent) return;
            agent.updateRotation = false;
            agent.updatePosition = true;
        }

        IEnumerator Start() {
            var lastPosition = transform.position;
            var (delay,threshold,radius) = (1f, 1f, 0.5f);
            while (true) {
                yield return new WaitForSeconds(delay);
                var position = (WalkTarget)
                    ? WalkTarget.position
                    : transform.position;
                if (threshold>(position-lastPosition).sqrMagnitude) continue;
                var v = UnityEngine.Random.insideUnitCircle.normalized*radius;
                WalkTarget.position = position+new Vector3(v.x,0,v.y);
                lastPosition = position;
            }
        }

        void FixedUpdate() {
            if (EquippedItem && hand) {
                EquippedItem.transform.position = hand.position;
                EquippedItem.transform.rotation = hand.rotation;
            } if (!agent || !agent.enabled) return;
            agent.SetDestination(WalkTarget.position);
            motor.Move(
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
            if (LookTarget != null) {
                animator.SetLookAtWeight(0.5f);
                animator.SetLookAtPosition(LookTarget.position);
            }
        }

        void OnCollisionEnter(Collision o) => Kill(
            force: o.impulse/Time.fixedDeltaTime,
            position: o.contacts.FirstOrDefault().point);


        public void Kill(Vector3 force, Vector3 position) {
            if (force.sqrMagnitude<100) return;
            if (Get<ai::NavMeshAgent>()) Get<ai::NavMeshAgent>().enabled = false;
            if (Get<Animator>()) Get<Animator>().enabled = false;
            if (Get<Collider>()) Get<Collider>().enabled = false;
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
                        current: LookTarget.position,
                        target: location.position,
                        currentVelocity: ref speed,
                        smoothTime: 1));
                yield return new WaitForSeconds(3);
            }
        }

        public void TravelTo(Transform location) {
            motor.IsDisabled = true;
            if (agent) agent.enabled = false;
            transform.position = location.position;
            if (Physics.Raycast(
                origin: transform.position,
                direction: Vector3.down,
                hitInfo: out RaycastHit hit,
                maxDistance: 1)) transform.position = hit.point;
            WalkTarget = location;
            if (agent) agent.enabled = true;
            motor.IsDisabled = false;
            motor.Get<Rigidbody>().AddForce(Physics.gravity);
        }


        public void Do(Cond cond, Action then) {
            StartSemaphore(Doing);
            IEnumerator Doing() {
                yield return new WaitWhile(() => cond());
                yield return new WaitForSeconds(0.5f);
                then();
            }
        }

        void OnSit() {
            Log(Description["sit"]);
            if (animator) animator.SetBool("Sit",true); }

        public override void Stand() { base.Stand();
            Log(Description["stand"]);
            if (animator) animator.SetBool("Sit",false); }

        public virtual void Stand(Thing thing, StoryArgs args) {
            if (animator.GetBool("Sit")) (thing as Actor).Stand();
            else throw new StoryError(thing.Description["try to stand"]); }

        public override void Pray() { base.Pray();
            if (animator) animator.SetBool("Pray", true); }

        new public class Data : Actor.Data {
            public override BaseObject Deserialize(BaseObject o) {
                var instance = base.Deserialize(o) as Person;
                return instance;
            }
        }
    }
}
