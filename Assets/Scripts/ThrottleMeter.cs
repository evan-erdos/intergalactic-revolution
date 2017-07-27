
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

public class ThrottleMeter : Adventure.Object {
    [SerializeField] int length = 20;
    [SerializeField] float step = 0.1f;
    Vector3[] points;
    Spaceship ship;
    LineRenderer line;

    void Awake() {
        (ship, line) = (GetParent<Spaceship>(), Get<LineRenderer>());
        points = new Vector3[length+1];
        for (var i=0;i<length+1;++i) points[i] = new Vector3(i*step,0,0);
        line.SetPositions(points);
    }

    void LateUpdate() { line.positionCount = Ratio; line.SetPositions(points); }
    int Ratio => Mathf.Clamp((int) (length*ship.Thrust)+1, 0, length+1);
}
