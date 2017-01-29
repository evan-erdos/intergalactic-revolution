/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-11-23 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Puzzles {
    public class Lever : Piece<int>, IPushable {
        float theta, speed, squeeze, delay = 4f;
        GameObject arm, handle;
        new AudioSource audio;
        [SerializeField] Vector2 range = new Vector2(-20,20);
        [SerializeField] Vector2 grip = new Vector2(0,-15);
        [SerializeField] protected AudioClip soundLever, soundHandle;
        public bool IsInitSolved {get;protected set;}
        public bool IsLocked {get;protected set;}
        public int Selections => 6;
        public float Theta {
            get { return theta; }
            set { theta = Mathf.Clamp(value, range.x, range.y); } }

        public override void Use() { if (IsSolved) Push(); else Pull(); }

        void Start() => squeeze = grip.y;

        void FixedUpdate() {
            if (IsLocked) return;
            var rotation = Quaternion.Euler(0f,0f,theta);
            arm.transform.localRotation = Quaternion.Slerp(
                arm.transform.localRotation,
                rotation, Time.deltaTime*5f);
            var angle = Quaternion.Euler(0f,0f,squeeze);
            handle.transform.localRotation = Quaternion.Slerp(
                handle.transform.localRotation,
                angle, Time.deltaTime*8f);
        }

        public virtual void Push() {
            audio.PlayOneShot(soundLever,0.2f);
            Log($"<cmd>You pull the {Name} back.</cmd>");
            Solve(Condition+1);
        }

        public void Pull(Actor actor, StoryArgs args) {
            StartCoroutine(Pulling(!IsSolved));
            IEnumerator Pulling(bool t) {
                if (t) Pull();
                else Push();
                yield return new WaitForSeconds(delay);
            }
        }

        public virtual void Pull() {
            audio.PlayOneShot(soundLever,0.2f);
            Log($"<cmd>You pull the {Name} forwards.</cmd>");
            Solve(Condition-1);
        }


        protected override void OnSolve() =>
            Log("You hear the sound of stone grinding in the distance.");


        public override int Pose() {
            return Posing().Current;
            IEnumerator<int> Posing() {
                var increment = 1;
                while (true) {
                    if (Condition>=Selections) increment = -1;
                    else if (0>=Condition) increment = 1;
                    yield return Condition += increment;
                }
            }
        }

        public override bool Solve(int condition) {
            var wasSolved = IsSolved;
            Condition = condition;
            StartCoroutine(Solving());
            // if (IsSolved!=wasSolved) Solve();
            return IsSolved;
            IEnumerator Solving() { yield break; }
        }

        protected override void Awake() { base.Awake();
            audio = GetComponent<AudioSource>();
            arm = transform.FindChild("arm").gameObject;
            handle = arm.transform.FindChild("handle").gameObject;
            if (range.x>range.y) range = new Vector2(range.y,range.x);
        }

        new public class Data : Piece<float>.Data {
            public override BaseObject Deserialize(BaseObject o) {
                var instance = base.Deserialize(o) as Lever;
                return instance;
            }
        }
    }
}
