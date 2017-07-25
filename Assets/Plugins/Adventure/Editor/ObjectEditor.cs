using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Adventure.Object))] public class ObjectEditor : CustomInspectorEditor {
    string copyright = "\nAdventure Framework, Copyright Ben Scott, 2017";
    protected override void OnAfterDefaultInspector() => GUILayout.Label(copyright);
}
