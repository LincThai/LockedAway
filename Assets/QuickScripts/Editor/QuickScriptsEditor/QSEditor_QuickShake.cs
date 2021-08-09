//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

[CustomEditor(typeof(QuickShake))]
[CanEditMultipleObjects]
public class QSEditor_QuickShake : Editor
{
    static string helpText = "Quick Tips:" +
        "\n\n1.If you want this object to Ease In or Out over time, you will need to call the relevant public function either from a Unity Event or through your own code." +
        "\n\nFor more information on how to use this component, see the User Guide.";


    public bool showHelp;

    [SerializeField]
    QuickShake _quickShake;

    void OnEnable()
    {
        _quickShake = (MonoBehaviour)target as QuickShake;
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
        EditorGUILayout.LabelField("For Testing During PlayMode", EditorStyles.boldLabel);

        if (GUILayout.Button("Ease In"))
        {
            _quickShake.EaseIn();
        }

        if (GUILayout.Button("Ease Out"))
        {
            _quickShake.EaseOut();
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
