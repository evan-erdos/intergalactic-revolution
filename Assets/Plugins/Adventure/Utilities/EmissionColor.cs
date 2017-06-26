/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class EmissionColor : MonoBehaviour {
    [ColorUsageAttribute(true,true,0,8,0.125f,3)]
    [SerializeField] Color emission = new Color(1,1,1,1);
    Color color = Color.black;
    new Renderer renderer;

    public Color Emission {
        get { return renderer.material.GetColor("_EmissionColor"); }
        set { foreach (var o in renderer.materials) o.SetColor("_EmissionColor", value); } }

    void Awake() {
        (renderer, Emission) = (GetComponent<Renderer>(), emission);
        foreach (var o in renderer.materials) o.EnableKeyword("_EMISSION");
    }

    IEnumerator Start() {
        while (true) {
            Emission = Color.Lerp(Emission,color,1);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
