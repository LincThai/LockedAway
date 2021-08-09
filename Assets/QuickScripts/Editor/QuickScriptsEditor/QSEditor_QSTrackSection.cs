//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

[CustomEditor(typeof(QS_TrackSection))]
public class QSEditor_QSTrackSection : Editor
{
    [SerializeField]
    QS_TrackSection _trackSection;

    void OnEnable()
    {
        _trackSection = (MonoBehaviour)target as QS_TrackSection;
        _trackSection.selectedInEditor = true;
    }

    private void OnDisable()
    {
        _trackSection.selectedInEditor = false;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Label("Track Construction (No Undo)", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Track Section"))
        {
            _trackSection.parentTrack.AddTrackSection();
        }

        if (GUILayout.Button("Insert Track Section")) {
            _trackSection.parentTrack.InsertTrack(_trackSection);
        }

        if (GUILayout.Button("Delete Last Track Section")) {
            Selection.activeGameObject = _trackSection.parentTrack.trackSections
                [_trackSection.parentTrack.trackSections.Count - 2].endNode.gameObject;

            _trackSection.parentTrack.DeleteLastTrack();
        }

        if (GUILayout.Button("Delete This Track Section")) {
            _trackSection.parentTrack.DeleteThisTrack(_trackSection);
        }

        GUILayout.Space(20);


        GUILayout.Label("Position Nodes (No Undo)", EditorStyles.boldLabel);

        if (GUILayout.Button("Point All Nodes At Next Handles"))
        {
            _trackSection.parentTrack.PointAllNodesAtHandles();
        }

        if (GUILayout.Button("Point All Nodes At Next Nodes"))
        {
            _trackSection.parentTrack.PointAllNodesAtNodes();
        }

        GUILayout.Space(20);
        GUILayout.Label("Position Handles", EditorStyles.boldLabel);


        if (GUILayout.Button("Align Curve Handle To Start Node"))
        {
            Transform startNode = _trackSection.startNode.transform;
            Undo.RecordObject(_trackSection.curveHandle.transform, "Align Curve Handle to Start Node");

            _trackSection.curveHandle.transform.position = startNode.position + startNode.forward * 2;
            _trackSection.curveHandle.transform.rotation = startNode.rotation;
        }

        if (GUILayout.Button("Align Curve Handle To End Node"))
        {
            Transform endNode = _trackSection.endNode.transform;
            Undo.RecordObject(_trackSection.curveHandle.transform, "Align Curve Handle to End Node");

            _trackSection.curveHandle.transform.position = endNode.position - endNode.forward * 2;
            _trackSection.curveHandle.transform.rotation = endNode.rotation;
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Snap To Start Node"))
        {
            _trackSection.transform.position = _trackSection.parentTrack.trackSections[0].startNode.transform.position;
            _trackSection.transform.rotation = _trackSection.parentTrack.trackSections[0].startNode.transform.rotation;
        }


        if (GUI.changed) //&& _trackSection != null) // is null if deleted
        {
            EditorUtility.SetDirty(_trackSection);
            serializedObject.ApplyModifiedProperties();
            
        }
    }
}