/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2014-07-06 */

using System.Collections;
using UnityEngine;
using Adventure.Inventories;

namespace Adventure.Locales {

    /// Door : Thing
    /// Any door which behaves in much the same manner as any other doors,
    /// and carries with it all the responsibilities one would typically
    /// associate with doors or other objects of a similar nature.
    public class Door : Path, IOpenable, ILockable {
        float velocity, delay, time = 4f;
        Vector3 initialDirection, openDirection, direction;
        Transform door, target;
        protected new Collider collider;
        protected new AudioSource audio;
        [SerializeField] protected AudioClip soundClick, soundOpen;
        [SerializeField] protected StoryEvent onOpen, onShut;
        public event StoryAction OpenEvent, ShutEvent;
        public bool IsOpen {get;protected set;}
        public bool IsStuck {get;protected set;}
        public bool IsAutoClosing {get;protected set;}
        public bool IsLocked {get;protected set;}
        public bool IsInitOpen {get;protected set;}
        public Key LockKey {get;protected set;}
        public void Use() { if (IsOpen) Shut(); else Open(); }
        public void Open() => Open(null, new StoryArgs());
        public void Shut() => Shut(null, new StoryArgs());
        public void Open(Thing o, StoryArgs e) => OpenEvent?.Invoke(o,e);
        public void Shut(Thing o, StoryArgs e) => ShutEvent?.Invoke(o,e);

        void OnOpen() {
            StartSemaphore(Opening);
            IEnumerator Opening() {
                if (IsOpen) { Log(Description["already open"]); yield break; }
                Log(Description["open"]);
                yield return StartCoroutine(Moving(openDirection));
                if (!IsAutoClosing) yield break;
                yield return new WaitForSeconds(time);
                Shut();
            }
        }

        void OnShut() {
            StartSemaphore(Shutting);
            IEnumerator Shutting() {
                if (!IsOpen) { Log(Description["already shut"]); yield break; }
                Log(Description["shut"]);
                yield return StartCoroutine(Moving(initialDirection));
            }
        }

        IEnumerator Moving(Vector3 direction) {
            collider.enabled = false;
            audio.PlayOneShot(soundOpen,0.8f);
            var speed = Vector3.zero;
            if (!IsStuck) while (door.position!=direction) {
                yield return new WaitForFixedUpdate();
                delay = Mathf.SmoothDamp(
                    current: delay,
                    target: (IsStuck)?0f:0.6f,
                    currentVelocity: ref velocity,
                    smoothTime: 0.1f,
                    maxSpeed: 1f,
                    deltaTime: Time.fixedDeltaTime);
                door.position = Vector3.SmoothDamp(
                    current: door.position,
                    target: direction,
                    currentVelocity: ref speed,
                    smoothTime: 0.8f,
                    maxSpeed: delay);
            }
        }


        public void Lock(Thing thing) {
            if (!(thing is Key key))
                throw new StoryError(Description["not lock"]);
            if (key!=LockKey || key.Kind!=LockKey.Kind)
                throw new StoryError(Description["cannot lock"]);
        }

        public void Unlock(Thing thing) {
            var key = thing as Key;
            if (!IsLocked) throw new StoryError(Description["already unlocked"]);
            if (!key || key==LockKey) return;
            if (key.Kind!=LockKey.Kind) return;
            StartCoroutine(Unlocking());
            IEnumerator Unlocking() {
                audio.PlayOneShot(soundClick,0.8f);
                yield return new WaitForSeconds(0.25f);
            }
        }

        protected override void Awake() { base.Awake();
            audio = GetOrAdd<AudioSource>();
            collider = GetOrAdd<Collider,SphereCollider>();
            audio.clip = soundOpen;
            target = GetOrAdd("target");
            door = GetOrAdd("door");
            initialDirection = door.position;
            openDirection = target.position;
            direction = initialDirection;
            if (!IsInitOpen) return;
            direction = openDirection;
            door.position = direction;
            onOpen.AddListener((o,e) => OnOpen());
            onShut.AddListener((o,e) => OnShut());
            OpenEvent += (o,e) => onOpen?.Invoke(o,e);
            ShutEvent += (o,e) => onShut?.Invoke(o,e);
        }

        new public class Data : Thing.Data {
            public bool opened {get;set;}
            public bool stuck {get;set;}
            public bool initiallyOpened {get;set;}
            public bool locked {get;set;}
            public bool closeAutomatically {get;set;}
            public override BaseObject Deserialize(BaseObject o) {
                var door = base.Deserialize(o) as Door;
                door.IsOpen = this.opened;
                door.IsStuck = this.stuck;
                door.IsLocked = this.locked;
                door.IsInitOpen = this.initiallyOpened;
                door.IsAutoClosing = this.closeAutomatically;
                return door;
            }
        }
    }
}
