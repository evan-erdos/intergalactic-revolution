using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class CustomInspectorEditor : Editor {
    static readonly string[] hiddenProperties = new string[] { "m_Script" };
    protected string[] HiddenProperties => Enumerable.Concat(hiddenProperties, GetHiddenProperties()).ToArray();
    protected virtual IEnumerable<string> GetHiddenProperties() => new string[] { };

    public override void OnInspectorGUI() {
        serializedObject.Update();
        OnBeforeDefaultInspector();
        DrawPropertiesExcluding(serializedObject, HideProperties());
        OnAfterDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }

    protected virtual void OnBeforeDefaultInspector() { }
    protected virtual void OnAfterDefaultInspector() { }
    protected virtual string[] HideProperties() => HiddenProperties;
}
