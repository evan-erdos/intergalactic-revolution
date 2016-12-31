/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ShipManager : MonoBehaviour {
    bool once;
    [SerializeField] protected GameObject spaceCamera;

    void Awake() {
        if (!spaceCamera) return;
        var instance = Instantiate(spaceCamera) as GameObject;
        var camera = GetComponent<Camera>();
        var spacecam = instance.GetComponent<Camera>();
        spacecam.rect = camera.rect;
    }

    public void Restart() {
        if (once) return;
        once = true;
        StartCoroutine(Restarting());
    }

    IEnumerator Restarting() {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
