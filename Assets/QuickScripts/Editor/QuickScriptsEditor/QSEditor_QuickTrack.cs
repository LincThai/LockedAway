//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

[CustomEditor(typeof(QuickTrack))]
[CanEditMultipleObjects]
public class QSEditor_QuickTrack : Editor
{

    [SerializeField]
    QuickTrack _moverTrack;
    public bool showHelp;
    static string helpText = "Quick Tips:" +
        "\n\n1. Always add or delete your Quick Tracks using the buttons below or the buttons on the Track Sections.Do not delete Track Sections or sub game objects from the Hierarchy or you risk breaking the whole track!" +
        "\n\n2. There is no undo when using the Construction / Positioning buttons.Please keep this in mind." +
        "\n\n3. Quick Tracks contain Track Sections, which contain Track Nodes. Tracks, Sections and Nodes have their own customisation options so be sure to check them all!" +
        "\n\n4. If the editor is lagging and you are using many tracks or have one very very long track, try unticking ‘Draw Gizmos’" +
        "\n\nFor more information on how to use this component, see the User Guide.";


    void OnEnable()
    {
        _moverTrack = (MonoBehaviour)target as QuickTrack;
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
        GUILayout.Label("Track Construction (No Undo)", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Track Section"))
        {
            CreateTrackSection();
            _moverTrack.drawGizmos = true;
        }
        if (GUILayout.Button("Delete Last Track Section"))
        {
            _moverTrack.DeleteLastTrack();
        }
        if (GUILayout.Button("Rebuild List"))
        {
            _moverTrack.RefreshNodeList();
        }
       

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_moverTrack);
            serializedObject.ApplyModifiedProperties();
        }

    }

    void ToggleGizmos()
    {
        _moverTrack.drawGizmos = !_moverTrack.drawGizmos;
    }

    public void CreateTrackSection()
    {
        // Set up required variables
        _moverTrack = (MonoBehaviour)target as QuickTrack;
        
        _moverTrack.AddTrackSection();

    }

    void SetGizmo(QuickGizmo gizmo)
    {
        Color newColor = Color.cyan;
        gizmo.gizmoColor = newColor;
        gizmo.gizmoColor.a = 0.4f;
        gizmo.gizmoType = QuickGizmo.currentGizmoType.sphere;
        gizmo.gizmoRadius = 0.4f;
    }


    void OpenUserGuide()
    {
        System.Diagnostics.Process.Start(Application.dataPath + "/QuickScripts/QuickScriptsUserGuide.pdf");
    }


}
