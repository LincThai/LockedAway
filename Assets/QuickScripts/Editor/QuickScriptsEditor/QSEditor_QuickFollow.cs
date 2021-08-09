//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

[CustomEditor(typeof(QuickFollow))]
[CanEditMultipleObjects]
public class QSEditor_QuickFollow : Editor
{
    static string helpText = "Quick Tips:" +
        "\n\n1. Drag and drop any game object in the Scene into the Target field.This game object will now follow the Target. " +
        "\n\n2. If ‘Stick to Target’ is true, this game object will copy the world position of the Target game object every frame." +
        "\n\n3. If you want this game object to slowly follow the Target, set a dampening speed. If the Target is also moving slowly you will need a very high damping value, for example 0.99. " +
        "\n\n4. Experiment with the rotation options if you want the following to look more natural." +
        "\n\nFor more information on how to use this component, see the User Guide.";
    public bool showHelp;

    [SerializeField]
    QuickFollow _quickFollow;

    void OnEnable()
    {
        _quickFollow = (MonoBehaviour)target as QuickFollow;
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
        

        if (GUILayout.Button("Snap To Position"))
        {
            if (_quickFollow.target != null) 
            _quickFollow.transform.position = _quickFollow.target.position;
        }

    }

    void OpenUserGuide()
    {
        System.Diagnostics.Process.Start(Application.dataPath + "/QuickScripts/QuickScriptsUserGuide.pdf");
    }
}
