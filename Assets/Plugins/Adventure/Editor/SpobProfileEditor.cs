using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpobProfile), true)]
public class SpobProfileEditor : ProfileEditor {
    protected override IEnumerable<string> GetHiddenProperties() => new string[] { "Name" };

    protected override void OnBeforeDefaultInspector() {
        var oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>((target as SpobProfile).Name);
        serializedObject.Update(); EditorGUI.BeginChangeCheck();
        var newScene = EditorGUILayout.ObjectField("Name", oldScene, typeof(SceneAsset), false) as SceneAsset;
        if (EditorGUI.EndChangeCheck()) {
            var path = AssetDatabase.GetAssetPath(newScene);
            var property = serializedObject.FindProperty("Name");
            property.stringValue = path;
        } serializedObject.ApplyModifiedProperties();
    }
}
