using UnityEngine;
using System.Collections;

public class UniqueShadowSun : MonoBehaviour {
    public static Light instance;
    void OnEnable() { if (instance) return; instance = GetComponent<Light>(); }
    void OnDisable() { if (instance!=null && instance==GetComponent<Light>()) instance = null; }
}
