/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-10-27 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Movement {
    public class Feet : MonoBehaviour {
        bool isDashing, isDucking, isLanding;
        Semaphore semaphore;
        float volume = 0.6f;
        Transform last;
        new AudioSource audio;
        public AudioClip[] stepSounds;
        Map<RandList<AudioClip>> sounds = new Map<RandList<AudioClip>> {
            ["Dirt"] = new RandList<AudioClip>(),
            ["Gravel"] = new RandList<AudioClip>(),
            ["Puddle"] = new RandList<AudioClip>(),
            ["Sand"] = new RandList<AudioClip>(),
            ["Swamp"] = new RandList<AudioClip>(),
            ["Water"] = new RandList<AudioClip>(),
            ["Wood"] = new RandList<AudioClip>(),
            ["Glass"] = new RandList<AudioClip>(),
            ["Concrete"] = new RandList<AudioClip>(),
            ["Default"] = new RandList<AudioClip>()};

        public float Volume => isDashing?0.2f:isDucking?0.05f:0.1f;
        public float Rate => isDashing?0.15f:isDucking?0.3f:0.2f;
        public bool HasLanded => !isLanding && HasMoved(0.2f);

        public void OnMove(Person actor, StoryArgs args) { }
        bool HasMoved(float d=0.4f) => transform.IsNear(last,d*d);

        public void Land(string step, float volume) {
            semaphore.Invoke(Landing);
            IEnumerator Landing() {
                if (isLanding) yield break;
                isLanding = true;
                audio.PlayOneShot(
                    sounds[step].Pick() ??
                    sounds["default"].Pick(),volume);
                var deviation = Random.Range(-0.005f, 0.05f);
                yield return new WaitForSeconds(Rate+deviation);
                last = transform;
                isLanding = false;
            }
        }

        public void Step(string step="default") {
            semaphore.Invoke(Stepping);
            IEnumerator Stepping() {
                isLanding = true;
                var deviation = Random.Range(-0.005f,0.05f);
                yield return new WaitForSeconds(Rate+deviation);
                isLanding = false;
                if (HasMoved()) Land(step, Volume);
            }
        }


        public void OnFootstep(PhysicMaterial physicMaterial) {
            var name = physicMaterial.name.ToLower();
            var list =
                from kind in sounds.Keys
                where name.Contains(kind.ToLower())
                select kind;
            if (!list.Any()) Step();
            if (!list.Many()) return;
            foreach (var sound in sounds.Keys)
                if (HasLanded) Land(sound,volume/list.Count());
                else Step(sound);

        }

        void Awake() {
            semaphore = new Semaphore(StartCoroutine);
            audio = GetComponent<AudioSource>();
            foreach (var kind in sounds.Keys)
                foreach (var sound in stepSounds)
                    if (sound.name.ToLower().Contains(kind))
                        sounds[kind].Add(sound);
        }

        void Update() {
            isDashing = Input.GetButtonDown("Dash");
            isDucking = Input.GetButtonDown("Duck");
        }

        void OnCollisionEnter(Collision collision) =>
            OnFootstep(collision.collider.material);
    }
}
