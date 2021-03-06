/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ui=UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Adventure {
    public class Loader : MonoBehaviour {
        float value, ratio, speed;
        [SerializeField] RealEvent onLoad = new RealEvent();
        void Update() => OnReal(ratio=Mathf.SmoothDamp(ratio,value,ref speed,0.1f));
        void OnReal(float o) => onLoad.Call(o);
        public void Load(params Scene[] scenes) => Load(() => OnReal(1),scenes);
        public void Load(params string[] scenes) => Load(() => OnReal(1),scenes);
        public void Load(Action then, params Scene[] scenes) => Load(then, scenes.Select(o => o.name).ToArray());
        public void Load(Action then, params string[] scenes) {
            StartCoroutine(Loading());
            IEnumerator Loading() {
                var current = 0;
                foreach (var scene in scenes) {
                    var task = SceneManager.LoadSceneAsync(scene,LoadSceneMode.Additive);
                    while (task?.isDone!=true) {
                        value = (current+task.progress)/scenes.Length; yield return null; }
                } yield return new WaitForSeconds(0.1f); then();
            }
        }
    }
}
