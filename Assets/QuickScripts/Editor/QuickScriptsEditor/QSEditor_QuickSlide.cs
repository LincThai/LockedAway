//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

[CustomEditor(typeof(QuickSlide))]
[CanEditMultipleObjects]
public class QSEditor_QuickSlide : Editor
{
    static string helpText = "Quick Tips:" +
        "\n\n1. Make sure you set both a Start Position and an End Position by moving the object where you want it and then pressing the relative button below." +
        "\n\n2. You can make use of Public Functions via any Event System (Quick Trigger, Relays, etc) or through your own custom code to set new Start and End positions at runtime." +
        "\n\nFor more information on how to use this component, see the User Guide.";

    public bool showHelp;

    [SerializeField]
    QuickSlide _quickSlide;

    void OnEnable()
    {
        _quickSlide = (MonoBehaviour)target as QuickSlide;
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

        if (GUILayout.Button("Set Start Pos Here"))
        {
            _quickSlide.SetStartPosition(_quickSlide.transform.position);
            _quickSlide.SetStartRotation(_quickSlide.transform.rotation);
        }
        if (GUILayout.Button("Set End Pos Here"))
        {
            _quickSlide.SetEndPosition(_quickSlide.transform.position);
            _quickSlide.SetEndRotation(_quickSlide.transform.rotation);
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Go To Start Pos"))
        {
            _quickSlide.transform.position = _quickSlide.startPos;
            _quickSlide.transform.rotation = _quickSlide.startRot;
        }
        if (GUILayout.Button("Go To End Pos"))
        {
            _quickSlide.transform.position = _quickSlide.endPos;
            _quickSlide.transform.rotation = _quickSlide.endRot;
        }


            EditorGUILayout.Space();
        EditorGUILayout.LabelField("For Testing During PlayMode", EditorStyles.boldLabel);
        if (GUILayout.Button("Slide To End"))
        {
            _quickSlide.SlideToEnd();
        }
        if (GUILayout.Button("Slide To Start"))
        {
            _quickSlide.SlideToStart();
        }

    }

    void OpenUserGuide()
    {
        System.Diagnostics.Process.Start(Application.dataPath + "/QuickScripts/QuickScriptsUserGuide.pdf");
    }
}
