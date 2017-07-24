/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2014-07-06 */

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Adventure.Inventories;

namespace Adventure.Locales {

    /// Door : Thing
    /// Any door which behaves in much the same manner as any other doors,
    /// and carries with it all the responsibilities one would typically
    /// associate with doors or other objects of a similar nature.
    public class Door : Path, IOpenable, ILockable {
        float velocity, delay, time=4;
        Vector3 direction, initDirection, openDirection;
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

        async Task OnOpen(IThing o, StoryArgs e) {
            if (IsOpen) { Log(Description["already open"]); return; }
            Log(Description["open"]); await Moving(openDirection);
            if (IsAutoClosing) { await time; Shut(); }
        }

        async Task OnShut(IThing o, StoryArgs e) {
            if (!IsOpen) { Log(Description["already shut"]); return; }
            Log(Description["shut"]); await Moving(initDirection);
        }

        async Task Moving(Vector3 direction) {
            var speed = Vector3.zero;
            collider.enabled = false;
            audio.PlayOneShot(soundOpen,0.8f);
            if (!IsStuck) while (door.position!=direction) {
                await 0;
                delay = Mathf.SmoothDamp(
                    current: delay, target: (IsStuck)?0f:0.6f,
                    currentVelocity: ref velocity, smoothTime: 0.1f,
                    maxSpeed: 1, deltaTime: Time.fixedDeltaTime);
                door.position = Vector3.SmoothDamp(
                    current: door.position, target: direction,
                    currentVelocity: ref speed, smoothTime: 0.8f,
                    maxSpeed: delay, deltaTime: Time.fixedDeltaTime);
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
            if (!key || key==LockKey || key.Kind!=LockKey.Kind) return;
            StartAsync(Unlocking);
            async Task Unlocking() { audio.PlayOneShot(soundClick,0.8f); await 0.25; }
        }

        protected override void Awake() { base.Awake();
            (audio, collider) = (GetOrAdd<AudioSource>(), GetOrAdd<Collider,SphereCollider>());
            audio.clip = soundOpen;
            (target, door) = (GetOrAdd("target"), GetOrAdd("door"));
            (initDirection, openDirection) = (door.position, target.position);
            direction = initDirection;
            if (!IsInitOpen) return;
            direction = door.position = openDirection;
            onOpen.AddListener((o,e) => StartAsync(() => OnOpen(o,e)));
            onShut.AddListener((o,e) => StartAsync(() => OnShut(o,e)));
            OpenEvent += (o,e) => onOpen?.Invoke(o,e);
            ShutEvent += (o,e) => onShut?.Invoke(o,e);
        }

        new public class Data : Thing.Data {
            public bool opened {get;set;}
            public bool stuck {get;set;}
            public bool initiallyOpened {get;set;}
            public bool locked {get;set;}
            public bool closeAutomatically {get;set;}
            public override Object Deserialize(Object o) {
                var door = base.Deserialize(o) as Door;
                (door.IsOpen, door.IsInitOpen) = (this.opened, this.initiallyOpened);
                (door.IsStuck, door.IsLocked) = (this.stuck, this.locked);
                door.IsAutoClosing = this.closeAutomatically;
                return door;
            }
        }
    }
}
