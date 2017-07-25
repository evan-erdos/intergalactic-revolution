using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure;
using Adventure.Astronautics.Spaceships;

public class Warhead : Adventure.Object {
    void OnCollisionEnter(Collision o) => Get<Spaceship>().Kill();
}
