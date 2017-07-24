/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using UnityEngine;

class DestroyOnDelay : MonoBehaviour {
    [SerializeField] float delay = 1;
    async void Start() { await delay; Destroy(gameObject); }
}
