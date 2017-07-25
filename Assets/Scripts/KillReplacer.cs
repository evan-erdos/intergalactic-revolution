using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure;
using Adventure.Astronautics.Spaceships;

public class KillReplacer : Adventure.Object {
    [SerializeField] protected Transform replacement;
    void Awake() => GetParent<Spaceship>().KillEvent += (o,e) => OnKill();
    void OnKill() {
        replacement.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
