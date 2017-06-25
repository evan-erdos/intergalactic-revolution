
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereLine : MonoBehaviour {
    [SerializeField] float radius = 2.5f;

    IEnumerator Start() {
        var sides = 24;
        var (angle, hemi) = (2*Mathf.PI/sides, Mathf.PI/sides);
        var renderer = GetComponent<LineRenderer>();
        var (circle,sphere) = (new List<(float,float)>(),new List<Vector3>());
        var semi = new List<(float,float)>();
        for (var i=0;i<sides+1;++i)
            semi.Add((Mathf.Cos(i*hemi)*radius,Mathf.Sin(i*hemi)*radius));
        for (var i=0;i<sides+1;++i)
            circle.Add((Mathf.Cos(i*angle)*radius,Mathf.Sin(i*angle)*radius));
        for (var i=0;i<sides+1;++i)
            sphere.Add(new Vector3(semi[i].Item1, 0, semi[i].Item2));
        for (var i=0;i<sides+1;++i)
            sphere.Add(new Vector3(circle[i].Item1, circle[i].Item2, 0));
        for (var i=0;i<sides+1;++i)
            sphere.Add(new Vector3(0, semi[i].Item1, semi[i].Item2));

        renderer.positionCount = sphere.Count;
        renderer.SetPositions(sphere.ToArray());
        yield return new WaitForEndOfFrame();
    }
}
