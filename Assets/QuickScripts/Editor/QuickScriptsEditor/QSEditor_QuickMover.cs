//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

[CustomEditor (typeof(QuickMover))]
[CanEditMultipleObjects]
public class QSEditor_QuickMover : Editor {

	[SerializeField]
	QuickMover _quickMover;
	public bool showHelp;
    static string helpText = "Quick Tips:" +
        "\n\n1. Drag and drop any Quick Track from the Scene into the Quick Track field below, then press the ‘Go to Start Node’ button to move this Quick Mover into position." +
        "\n\n2. If you want this Quick Mover to start at a different position along the track, you can use the Next Node or Previous Node buttons to move it." +
        "\n\n3. Make sure the ‘Can Move’ box is ticked otherwise your Quick Mover will stay still. " +
        "\n\n4. There are a lot of useful Public Functions you can run on the Quick Mover from Unity Events (Quick Trigger, Relays, etc) so be sure to have a look at those in the documentation or in the script file itself." +
        "\n\nFor more information on how to use this component, see the User Guide.";





    void OnEnable(){
		_quickMover = (MonoBehaviour)target as QuickMover;
	}

	public override void OnInspectorGUI()
	{
		showHelp = (bool)EditorGUILayout.Toggle ("Show Help", showHelp);
		if (showHelp)
		{
			EditorGUILayout.BeginFadeGroup (1);
			EditorGUILayout.HelpBox (helpText, MessageType.None);
			if (GUILayout.Button ("User Guide"))
				OpenUserGuide ();
			EditorGUILayout.EndFadeGroup ();
		}

        DrawDefaultInspector();

        if (_quickMover.currentTrack != null && _quickMover.startTrackSection == null)
            _quickMover.startTrackSection = _quickMover.currentTrack.trackSections[0];

        if (GUILayout.Button("Snap To Assigned Section"))
        {
            Undo.RegisterCompleteObjectUndo(_quickMover, "Snap To Assigned Section");
            _quickMover.SnapToAssignedStartSection();

            EditorUtility.SetDirty(_quickMover);
        }


        EditorGUILayout.Space();
        EditorGUILayout.Space();


        if (GUILayout.Button("Next Track Node"))
        {
            Undo.RegisterCompleteObjectUndo(_quickMover, "Snap To Next Node");



            QS_TrackSection currentSection = _quickMover.startTrackSection;
            QS_TrackSection nextSection = _quickMover.currentTrack.GetNextTrackSection
                (_quickMover.startTrackSection, false);

            if (currentSection == nextSection)
                _quickMover.SnapToTrackEnd(currentSection);
            else
                _quickMover.SnapToSectionStart(nextSection);

            EditorUtility.SetDirty(_quickMover);

        }

        if (GUILayout.Button("Prev Track Node"))
        {
            Undo.RegisterCompleteObjectUndo(_quickMover, "Snap To Prev Node");


            _quickMover.SnapToSectionStart(_quickMover.currentTrack.GetNextTrackSection
                (_quickMover.startTrackSection, true));

            EditorUtility.SetDirty(_quickMover);

        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("Snap To Track Start"))
        {
            Undo.RegisterCompleteObjectUndo(_quickMover, "Snap To Track Start");

            _quickMover.SnapToTrackStart();

            EditorUtility.SetDirty(_quickMover);

        }

        if (GUILayout.Button("Snap To Track End"))
        {
            Undo.RegisterCompleteObjectUndo(_quickMover, "Snap To Track End");

            _quickMover.SnapToTrackEnd(_quickMover.currentTrack.LastTrackSection());

            EditorUtility.SetDirty(_quickMover);

        }

        if (_quickMover.moveSpeed > 30)
        {
            EditorGUILayout.BeginFadeGroup(1);
            EditorGUILayout.HelpBox("Warning: Move speeds above 30 could produce unexpected results. The recommended maximum speed is 20.", MessageType.Warning);
            EditorGUILayout.EndFadeGroup();
        }
       

        if (GUI.changed)
		{
			EditorUtility.SetDirty (_quickMover);
			serializedObject.ApplyModifiedProperties ();
		}
	}

   

	void OpenUserGuide()
	{
		System.Diagnostics.Process.Start (Application.dataPath + "/QuickScripts/QuickScriptsUserGuide.pdf");
	}
}
