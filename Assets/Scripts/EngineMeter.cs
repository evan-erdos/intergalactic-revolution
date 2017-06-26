
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

public class EngineMeter : Adventure.Object {
    [SerializeField] int sides = 6;
    [SerializeField] float radius = 2;
    Vector3[] circle = new Vector3[7];
    new LineRenderer renderer;
    Spaceship spaceship;

    void Awake() => (renderer,spaceship) = (Get<LineRenderer>(),GetParent<Spaceship>());
    void Start() => circle = new Vector3[sides+1];
    void Update() {
        var ratio = ((spaceship.Energy-1)%spaceship.EnergyJump)/spaceship.EnergyJump;
        var angle = (2*Mathf.PI/sides) * ratio;
        for (var i=0;i<sides+1;++i) circle[i] = new Vector3(
            Mathf.Cos(i*angle)*radius, Mathf.Sin(i*angle)*radius, 0);
        renderer.positionCount = circle.Length;
    }

    void LateUpdate() => renderer.SetPositions(circle);
}
