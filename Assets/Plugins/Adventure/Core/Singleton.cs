/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2011-05-21 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
   protected virtual void OnApplicationQuit() => instance = null;
   protected static T instance; public static T Instance { get {
      if (!(instance is null)) return instance;
      if (!((instance = FindObjectOfType<T>()) is null)) return instance;
      var manager = GameObject.Find($"Manager") ?? new GameObject("Manager");
      instance = manager.GetComponent<T>();
      if (instance is null) manager.AddComponent<T>(); return instance; } } }
