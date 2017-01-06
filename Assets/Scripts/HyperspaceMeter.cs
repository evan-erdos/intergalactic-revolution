
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

public class HyperspaceMeter : SpaceObject {
    [SerializeField] float radius = 2;
    [SerializeField] int sides = 6;
    Vector3[] points = new Vector3[2];
    new LineRenderer renderer;
    Spaceship spaceship;

    float Ratio => spaceship.Energy / spaceship.EnergyJump; // EnergyJump

    void Awake() {
        renderer = Get<LineRenderer>();
        spaceship = GetComponentInParent<Spaceship>();
        // sides = (int) (spaceship.EnergyCapacity/spaceship.EnergyJump)+2;
        points = new Vector3[sides+1];
        var angle = 2*Mathf.PI/sides;
        for (var i=0;i<sides+1;++i) points[i] = new Vector3(
            Mathf.Cos(i*angle)*radius,Mathf.Sin(i*angle)*radius,0);
    }

    void Update() => renderer.numPositions = (int) Ratio + 1;
    void LateUpdate() => renderer.SetPositions(points);
}
