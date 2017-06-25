
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

public class VelocityMeter : SpaceObject {
    [SerializeField] int length = 10;
    [SerializeField] float step = 0.1f;
    Vector3[] points;
    Spaceship spaceship;
    new LineRenderer renderer;

    void Awake() {
        renderer = Get<LineRenderer>();
        spaceship = GetComponentInParent<Spaceship>();
        points = new Vector3[length+1];
        for (var i=0;i<length+1;++i) points[i] = new Vector3(i*step,0,0);
        renderer.SetPositions(points);
    }

    void Update() => renderer.positionCount =
        (int)(length*Mathf.Min(spaceship.TopSpeed,spaceship.Speed)/spaceship.TopSpeed)+1;
}
