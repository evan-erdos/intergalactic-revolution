/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-10-27 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Motion {
    public class Feet : Adventure.Object {
        bool dash, duck, isLanding;
        float volume = 0.6f;
        Transform last;
        new AudioSource audio;
        public AudioClip[] stepSounds;
        Map<List<AudioClip>> sounds = new Map<List<AudioClip>> {
            ["Dirt"] = new List<AudioClip>(), ["Gravel"] = new List<AudioClip>(),
            ["Puddle"] = new List<AudioClip>(), ["Sand"] = new List<AudioClip>(),
            ["Swamp"] = new List<AudioClip>(), ["Water"] = new List<AudioClip>(),
            ["Wood"] = new List<AudioClip>(), ["Glass"] = new List<AudioClip>(),
            ["Concrete"] = new List<AudioClip>(), ["Default"] = new List<AudioClip>()};

        public float Volume => dash?0.2f:duck?0.05f:0.1f;
        public float Rate => dash?0.15f:duck?0.3f:0.2f;
        public bool HasLanded => !isLanding && HasMoved(0.2f);

        public void OnMove(Person actor, StoryArgs args) { }
        bool HasMoved(float d=0.4f) => transform.IsNear(last,d*d);

        public void Land(string step, float volume=1) {
            StartSemaphore(Landing);
            IEnumerator Landing() {
                if (isLanding) yield break;
                isLanding = true;
                audio.PlayOneShot(sounds[step].Pick() ?? sounds["default"].Pick(), volume);
                var deviation = Random.Range(-0.005f, 0.05f);
                yield return new WaitForSeconds(Rate+deviation);
                last = transform;
                isLanding = false;
            }
        }

        public void Step(string step="default") {
            StartSemaphore(Stepping);
            IEnumerator Stepping() {
                isLanding = true;
                var deviation = Random.Range(-0.005f,0.05f);
                yield return new WaitForSeconds(Rate+deviation);
                isLanding = false;
                if (HasMoved()) Land(step, Volume);
            }
        }


        public void OnFootstep(PhysicMaterial o) {
            var list =
                from kind in sounds.Keys
                where o.name.ToLower().Contains(kind.ToLower()) select kind;
            if (!list.Any()) Step();
            if (!list.Many()) return;
            foreach (var sound in sounds.Keys)
                if (HasLanded) Land(sound,volume/list.Count()); else Step(sound);
        }

        void Awake() => audio = Get<AudioSource>();
        void Start() { foreach (var kind in sounds.Keys) foreach (var sound in stepSounds)
            if (sound.name.ToLower().Contains(kind)) sounds[kind].Add(sound); }

        void Update() => (dash, duck) = (Input.GetButton("Dash"), Input.GetButton("Duck"));
        void OnCollisionEnter(Collision o) => OnFootstep(o.collider.material);
    }
}
