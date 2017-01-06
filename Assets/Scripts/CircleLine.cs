
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleLine : MonoBehaviour {
    [SerializeField] float radius = 2.5f;
    void Start() {
        var (sides, angle) = (24, 2*Mathf.PI/24);
        var renderer = GetComponent<LineRenderer>();
        var circle = new Vector3[sides+1];
        for (var i=0;i<sides+1;++i) circle[i] = new Vector3(
            Mathf.Cos(i*angle)*radius, Mathf.Sin(i*angle)*radius, 0);
        renderer.numPositions = circle.Length;
        renderer.SetPositions(circle);
    }
}
