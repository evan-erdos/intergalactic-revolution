
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

public class HeadingMeter : Adventure.Object {
    enum Heading { Forward, Velocity };
    [SerializeField] float radius = 1f;
    [SerializeField] Heading heading = Heading.Forward;
    Vector3[] points = new Vector3[2];
    Spaceship spaceship;
    new LineRenderer renderer;

    void Awake() {
        spaceship = GetParent<Spaceship>();
        renderer = Get<LineRenderer>();
        (renderer.useWorldSpace, renderer.positionCount) = (true,2);
    }

    void Update() {
        (points[0], points[1]) = (transform.position, transform.position);
        switch (heading) {
            case Heading.Forward:
                points[1] += (spaceship.transform.forward)*radius; break;
            case Heading.Velocity:
                points[1] += spaceship.Velocity.normalized*radius;
                break;
        }
    }

    void LateUpdate() => renderer.SetPositions(points);
}
