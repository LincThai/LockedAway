//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

[CustomEditor(typeof(QuickRelay))]
[CanEditMultipleObjects]
public class QSEditor_QuickRelay : Editor
{
    static string helpText = "Quick Tips:" +
        "\n\n1. Test the Quick Relay by ticking ‘Run On Start’ to easily see if it is firing its Events when the game starts(It will fire its Events after the Delay In Seconds has passed)." +
        "\n\n2.Increase the Calls Until Run if you want the Quick Relay to require multiple Calls from objects in the scene before it fires its Events." +
        "\n\nFor more information on how to use this component, see the User Guide.";
    public bool showHelp;

    [SerializeField]
    QuickRelay _quickRelay;

    void OnEnable()
    {
        _quickRelay = (MonoBehaviour)target as QuickRelay;
    }

    public override void OnInspectorGUI()
    {
        showHelp = (bool)EditorGUILayout.Toggle("Show Help", showHelp);
        if (showHelp)
        {
            EditorGUILayout.BeginFadeGroup(1);
            EditorGUILayout.HelpBox(helpText, MessageType.None);
            if (GUILayout.Button("User Guide"))
                OpenUserGuide();
            EditorGUILayout.EndFadeGroup();
        }
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("For Testing During PlayMode", EditorStyles.boldLabel);
        if (GUILayout.Button("Test"))
        {
            _quickRelay.ForceRunImmediate();
        }

            if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    void OpenUserGuide()
    {
        System.Diagnostics.Process.Start(Application.dataPath + "/QuickScripts/QuickScriptsUserGuide.pdf");
    }
}
