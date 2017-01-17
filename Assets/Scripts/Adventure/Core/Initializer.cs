/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-15 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Adventure {
    public class Initializer : MonoBehaviour {
        void Start() => GetComponent<SceneLoader>().Load(() =>
            Destroy(gameObject), new[] {"Base", "Menu"}); } }
