
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

public class EngineStatus : SpaceObject {
    [SerializeField] int sides = 6;
    [SerializeField] float radius = 2;
    [SerializeField] float ratio = 1;
    List<(float,float)> circle = new List<(float,float)>();
    new LineRenderer renderer;
    Spaceship spaceship;

    void Awake() {
        renderer = Get<LineRenderer>();
        spaceship = GetComponentInParent<Spaceship>();
    }

    void Update() {
        ratio = spaceship.Energy/spaceship.EnginePower;
        var angle = (2*Mathf.PI / sides) * ratio; // (2*Mathf.PI * ratio) / sides;
        for (var i=0;i<sides+1;++i)
            circle.Add((Mathf.Cos(i*angle)*radius, Mathf.Sin(i*angle)*radius));
        var points = new Vector3[sides+1];
        for (var i=0;i<sides+1;++i)
            points[i] = new Vector3(circle[i].Item1, circle[i].Item2, 0f);
        renderer.numPositions = sides+1;
        renderer.SetPositions(points);
    }
}
