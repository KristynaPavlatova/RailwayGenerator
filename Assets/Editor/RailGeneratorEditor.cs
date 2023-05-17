using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RailGenerator))]
public class RailGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RailGenerator myTarget = (RailGenerator)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Railway (wood + rails)", EditorStyles.boldLabel);
        if (GUILayout.Button("Generate"))
        {
            myTarget.GenerateRailway();
        }
        if (GUILayout.Button("Delete"))
        {
            myTarget.DeleteRailway();
        }
        if (GUILayout.Button("Save"))
        {
            myTarget.SaveRailway();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Wood", EditorStyles.boldLabel);
        if (GUILayout.Button("Generate"))
        {
            myTarget.GenerateWood();
        }
        if (GUILayout.Button("Delete"))
        {
            myTarget.DeleteWood();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rails", EditorStyles.boldLabel);
        if (GUILayout.Button("Generate"))
        {
            myTarget.GenerateRails();
        }
        if (GUILayout.Button("Delete"))
        {
            myTarget.DeleteRails();
        }

        EditorGUILayout.Space();
        DrawDefaultInspector();

        EditorGUILayout.LabelField("Railway (wood + rails)", EditorStyles.boldLabel);
        if (GUILayout.Button("Delete GameObject From Scene"))
        {
            myTarget.DeleteObjectFromScene();
        }
    }
}
