using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorAlertPopUp : EditorWindow {

    void OnGUI()
    {
        EditorGUILayout.LabelField("Success! Sources copied to Resources.", EditorStyles.wordWrappedLabel);

        GUILayout.Space(70);

        if (GUILayout.Button("OK"))
        {
            AssetDatabase.Refresh();
            this.Close();
        }
    }

}
