
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

public class ThrottleMeter : Adventure.Object {
    [SerializeField] int length = 10;
    [SerializeField] float step = 0.1f;
    Vector3[] points;
    Spaceship spaceship;
    new LineRenderer renderer;

    void Awake() {
        points = new Vector3[length+1];
        for (var i=0;i<length+1;++i) points[i] = new Vector3(i*step,0,0);
        (spaceship, renderer) = (GetParent<Spaceship>(), Get<LineRenderer>());
    }

    void LateUpdate() { renderer.positionCount = Ratio; renderer.SetPositions(points); }
    int Ratio => Mathf.Min((int) (length*spaceship.Throttle)+1, length+1);
}
