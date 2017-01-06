
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

public class HealthMeter : SpaceObject {
    [SerializeField] int length = 2;
    [SerializeField] float step = 0.1f;
    Vector3[] points = new Vector3[7];
    new LineRenderer renderer;
    Spaceship spaceship;

    float Ratio => (spaceship.Health+1)/spaceship.MaxHealth;

    void Awake() {
        renderer = Get<LineRenderer>();
        spaceship = GetComponentInParent<Spaceship>();
        points = new Vector3[length];
        for (var i=0;i<length;++i) points[i] = new Vector3(i*step,0,0);
    }

    void Update() => renderer.numPositions = (int) (length*Mathf.Clamp(Ratio,0,1));
    void LateUpdate() => renderer.SetPositions(points);
}
