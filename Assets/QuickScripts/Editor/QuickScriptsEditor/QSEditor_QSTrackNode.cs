//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

[CustomEditor(typeof(QS_TrackNode))]
public class QSEditor_QSTrackNode : Editor
{
    [SerializeField]
    QS_TrackNode _moverNode;

    void OnEnable()
    {
        _moverNode = (MonoBehaviour)target as QS_TrackNode;
    }

   

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Label("Track Construction (No Undo)", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Track Section At End"))
        {
            Undo.RecordObject(_moverNode, "Add Track Section At End");

            _moverNode.myTrackSection.parentTrack.AddTrackSection();
        }

        if (GUILayout.Button("Insert Track Section Here"))
        {
            Undo.RecordObject(_moverNode, "Insert Track Section Here");


            _moverNode.myTrackSection.parentTrack.InsertTrack(_moverNode.myTrackSection);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Delete This Track Section"))
        {
            Undo.RecordObject(_moverNode, "Delete This Track Section");


            _moverNode.myTrackSection.parentTrack.DeleteThisTrack(_moverNode.myTrackSection);
        }
        if (GUILayout.Button("Delete End Track Section")) {

            Undo.RecordObject(_moverNode, "Delete End Track Section");


            Selection.activeGameObject = _moverNode.myTrackSection.parentTrack.trackSections
                [_moverNode.myTrackSection.parentTrack.trackSections.Count - 2].endNode.gameObject;

            _moverNode.myTrackSection.parentTrack.DeleteLastTrack();
        }
        

        GUILayout.Space(20);
        GUILayout.Label("Position Nodes (No Undo)", EditorStyles.boldLabel);
        if (GUILayout.Button("Point This Node At Handle")) {

            _moverNode.myTrackSection.parentTrack.PointNodeAtHandle(_moverNode, _moverNode.myTrackSection);
        }

        if (GUILayout.Button("Point This Node At Next Node")) {
            _moverNode.myTrackSection.parentTrack.PointNodeAtNode(_moverNode, _moverNode.myTrackSection);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Point All Nodes At Next Handles"))
        {
            _moverNode.myTrackSection.parentTrack.PointAllNodesAtHandles();
        }

        if (GUILayout.Button("Point All Nodes At Next Nodes"))
        {
            _moverNode.myTrackSection.parentTrack.PointAllNodesAtNodes();
            EditorUtility.SetDirty(this);
        }

        GUILayout.Space(20);
        GUILayout.Label("Position Handles", EditorStyles.boldLabel);
        

        if (GUILayout.Button("Align Curve Handle To Start Node"))
        {

            Transform startNode = _moverNode.myTrackSection.startNode.transform;
            Undo.RecordObject(_moverNode.myTrackSection.curveHandle.transform, "Align Curve Handle to Start Node");

            _moverNode.myTrackSection.curveHandle.transform.position = startNode.position + startNode.forward * 2;
            _moverNode.myTrackSection.curveHandle.transform.rotation = startNode.rotation;

            EditorUtility.SetDirty(_moverNode.myTrackSection.curveHandle.transform);
        }

        //GUILayout.Space(5);
        //if (GUILayout.Button("Rotate Curve Handle to Match End Node"))
        //{
        //    Undo.RecordObject(_moverNode, "Rotate Curve Handle to Match End Node");
        //
        //    _moverNode.myTrackSection.curveHandle.transform.rotation = _moverNode.myTrackSection.endNode.transform.rotation;
        //}

        if (GUILayout.Button("Align Curve Handle To End Node"))
        {

            Transform endNode = _moverNode.myTrackSection.endNode.transform;
            Undo.RecordObject(_moverNode.myTrackSection.curveHandle.transform, "Align Curve Handle to End Node");

            _moverNode.myTrackSection.curveHandle.transform.position = endNode.position - endNode.forward * 2;
            _moverNode.myTrackSection.curveHandle.transform.rotation = endNode.rotation;

            EditorUtility.SetDirty(_moverNode.myTrackSection.curveHandle.transform);

        }

        GUILayout.Space(20);

        if (GUILayout.Button("Snap To Track Start Node"))
        {
            Undo.RecordObject(_moverNode.transform, "Snap To Track Start Node");


            _moverNode.transform.position = _moverNode.myTrackSection.parentTrack.trackSections[0].startNode.transform.position;
            _moverNode.transform.rotation = _moverNode.myTrackSection.parentTrack.trackSections[0].startNode.transform.rotation;
            EditorUtility.SetDirty(_moverNode.transform);

        }


        if (GUI.changed) //&& _moverNode != null) // is null if deleted
        {

            EditorUtility.SetDirty(_moverNode);
            serializedObject.ApplyModifiedProperties();
        }
    }
}