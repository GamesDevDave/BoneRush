/// <summary>
/// Created by Christopher Robertson
/// 
/// Place this script in Assets\Editor
/// </summary>

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrefabNumbering))]
public class PrefabNumberingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PrefabNumbering numbering = (PrefabNumbering)target;

        if (GUILayout.Button("Rename all Prefabs"))
        {
            numbering.RenameAllGameObjects();
        }
    }
}
