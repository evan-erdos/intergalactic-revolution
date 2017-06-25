using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure;

public class LineRatio : MonoBehaviour, IProportion {
    new LineRenderer renderer;
    public float Ratio {get;set;}
    void Awake() => renderer = GetComponent<LineRenderer>();
    void Start() => renderer.positionCount = 2;
    void Update() => renderer.SetPositions(new[] {
        new Vector3(0,0,0), new Vector3(Mathf.Min(1,Ratio)*10,0,0)});
}
