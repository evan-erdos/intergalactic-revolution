
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

public class BlasterMeter : Adventure.Object {
    [SerializeField] int sides = 6;
    [SerializeField] float radius = 2;
    [SerializeField] float ratio = 1;

    IEnumerator Start() {
        var circle = new List<Vector3>();
        var renderer = Get<LineRenderer>();
        var spaceship = GetComponentInParent<Spaceship>();
        while (true) {
            ratio = spaceship.Energy/spaceship.EnergyCapacity;
            var angle = (2*Mathf.PI/sides) * ratio;
            circle.Clear();
            for (var i=0;i<sides+1;++i) circle.Add(new Vector3(
                Mathf.Cos(i*angle)*radius,Mathf.Sin(i*angle)*radius,0));
            renderer.positionCount = sides+1;
            renderer.SetPositions(circle.ToArray());
            yield return new WaitForEndOfFrame();
        }
    }
}
