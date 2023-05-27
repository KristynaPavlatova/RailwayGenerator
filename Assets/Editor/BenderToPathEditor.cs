using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(BenderToPath))]
public class BenderToPathEditor : Editor
{
    bool showBendButton = true;
    bool showAdditionalButtons = false;
    bool showDefaultInspector = true;

    string bendButtonName = "Bend";

    /// <summary>
    /// Called every time the inspector is drawn inside Unity.
    /// </summary>
    public override void OnInspectorGUI()
    {
        BenderToPath myTarget = (BenderToPath)target;

        EditorGUILayout.Space(10);
        switch (myTarget.bendingStatus)
        {
            case BenderToPath.BendingStatus.WAITING_FOR_ACTION:
                GUI.contentColor = Color.white;
                EditorGUILayout.LabelField($"Status: {myTarget.bendingStatus}", EditorStyles.boldLabel);
                
                showBendButton = true;
                showAdditionalButtons = false;
                showDefaultInspector = true;

                bendButtonName = "Bend";
                break;
            case BenderToPath.BendingStatus.SUCCESS:
                GUI.contentColor = Color.green;
                EditorGUILayout.LabelField($"Status: {myTarget.bendingStatus}", EditorStyles.boldLabel);
                GUI.contentColor = Color.white;

                showBendButton = true;
                showAdditionalButtons = true;
                showDefaultInspector = true;

                bendButtonName = "Bend";
                break;
            case BenderToPath.BendingStatus.FAILED:
                GUI.contentColor = Color.red;
                EditorGUILayout.LabelField($"Status: {myTarget.bendingStatus}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Read the Console for more information.");
                GUI.contentColor = Color.white;

                showBendButton = true;
                showAdditionalButtons = false;
                showDefaultInspector = true;

                bendButtonName = "Try again";
                break;
            case BenderToPath.BendingStatus.IN_PROGRESS:
                GUI.contentColor = Color.yellow;
                EditorGUILayout.LabelField($"Status: {myTarget.bendingStatus}", EditorStyles.boldLabel);

                showBendButton = false;
                showAdditionalButtons = false;
                showDefaultInspector = false;

                bendButtonName = "Bend";
                break;
            default:
                break;
        }

        // Buttons
        if (showBendButton)
        {
            EditorGUILayout.Space(10);
            if (GUILayout.Button(bendButtonName))
            {
                myTarget.Bend();
            }

            if (showAdditionalButtons)
            {
                EditorGUILayout.Space(10);
                if (GUILayout.Button("Save"))
                {
                    myTarget.Save();
                }
                if (GUILayout.Button("Delete"))
                {
                    myTarget.Delete();
                    showAdditionalButtons = false;
                }
            }
        }

        EditorGUILayout.Space(10);
        if(showDefaultInspector)
            DrawDefaultInspector();
    }
}
