
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLine : MonoBehaviour {
    [SerializeField] float radius = 2.5f;

    void Start() {
        var (sides, angle) = (24, 2*Mathf.PI/24);
        var renderer = GetComponent<LineRenderer>();
        var rigidbody = GetComponent<Rigidbody>();
        var (circle,sphere) = (new List<(float,float)>(),new List<Vector3>());
        for (var i=0;i<sides+1;++i)
            circle.Add((Mathf.Cos(i*angle)*radius,Mathf.Sin(i*angle)*radius));
        for (var i=0;i<sides+1;++i)
            sphere.Add(new Vector3(circle[i].Item1, circle[i].Item2, 0));
        for (var i=0;i<sides+1;++i)
            sphere.Add(new Vector3(circle[i].Item1, 0, circle[i].Item2));
        for (var i=0;i<sides+1;++i)
            sphere.Add(new Vector3(0, circle[i].Item1, circle[i].Item2));

        renderer.positionCount = sphere.Count;
        renderer.SetPositions(sphere.ToArray());
    }

    void FixedUpdate() => transform.rotation = Quaternion.Euler(0,0,0); // -45
}
