/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-11-23 */

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Adventure.Puzzles {
    public class Lever : Piece<int,bool>, IPushable {
        float theta, speed, delay = 4;
        Transform arm, handle;
        new AudioSource audio;
        [SerializeField] Vector2 range = new Vector2(-20,20);
        [SerializeField] Vector2 grip = new Vector2(0,-15);
        [SerializeField] protected AudioClip soundLever, soundHandle;
        public bool IsInitSolved {get;protected set;}
        public bool IsLocked {get;protected set;}
        public int Selections => 6;
        public float Theta {
            get { return theta; }
            set { theta = Mathf.Clamp(value,range.x,range.y); } }

        public override void Init() { base.Init();
            onSolve.AddListener((o,e) => OnSolve(o,e)); }

        public override void Use() { if (IsSolved) Push(); else Pull(); }

        void FixedUpdate() {
            if (IsLocked) return;
            arm.localRotation = Quaternion.Slerp(arm.localRotation,
                Quaternion.Euler(0,0,theta), Time.deltaTime*5);
            handle.localRotation = Quaternion.Slerp(handle.localRotation,
                Quaternion.Euler(0,0,grip.y),Time.deltaTime*8);
        }

        public virtual void Push() {
            audio.PlayOneShot(soundLever,0.2f);
            Log($"<cmd>You pull the {Name} back.</cmd>");
            Solve(Condition+1);
        }

        async void OnPull(Actor actor, StoryArgs args) {
            if (!IsSolved) Pull(); else Push(); await delay; }

        public virtual void Pull() {
            audio.PlayOneShot(soundLever,0.2f);
            Log($"<cmd>You pull the {Name} forwards.</cmd>");
            Solve(Condition-1);
        }


        void OnSolve(IThing o, StoryArgs e) =>
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
            audio = Get<AudioSource>();
            (arm, handle) = (transform.Find("arm"), arm.Find("handle"));
            if (range.x>range.y) range = new Vector2(range.y, range.x);
        }

        new public class Data : Piece<int,bool>.Data {
            public override Object Deserialize(Object o) {
                var instance = base.Deserialize(o) as Lever;
                return instance;
            }
        }
    }
}
