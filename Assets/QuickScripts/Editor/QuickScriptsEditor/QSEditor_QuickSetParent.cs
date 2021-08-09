//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

[CustomEditor (typeof (QuickSetParent))]
[CanEditMultipleObjects]
public class QSEditor_QuickSetParent : Editor {

	[SerializeField]
	QuickSetParent _quickSetParent;
	SerializedProperty tagTransformList;

    static string helpText = "Quick Tips:" +
        "\n\n1. The Transform attached to the Quick SetParent should always have a scale of 1,1,1 unless you have set a custom Parent Transform(in which case the new Parent Transform’s scale should be 1,1,1). If it is not a scale of 1,1,1 very strange things may happen to the objects being childed to the transform.You have been warned!" +
        "\n\n2. The Transform being used as the parent(in Parent Transform) needs a Collider set to ‘is Trigger’. This collider is used as a trigger to catch / release all objects stepping in and out of the collider bounds." +
        "\n\n3. All objects you want the Quick SetParent to detect must have a Rigidbody." +
        "\n\n4. Tick ‘Edit Tags’ and add the relevant Tags you want the Quick SetParent to detect. Click Add Tag to automatically add them to the list under Tags and Transforms." +
        "\n\nFor more information on how to use this component, see the User Guide.";

    public bool showHelp;
	public bool showTags;
	string selectedTag;

	void OnEnable() {
		_quickSetParent = (MonoBehaviour)target as QuickSetParent;
		tagTransformList = serializedObject.FindProperty ("tagsAndTransforms");
	}

	public override void OnInspectorGUI ()
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

		if (_quickSetParent.parentTransform == null)
			_quickSetParent.parentTransform = _quickSetParent.transform;

		tagTransformList.isExpanded = true;
		//EditorGUILayout.PropertyField (tagTransformList);

		showTags = (bool)EditorGUILayout.Toggle ("Edit Tags", showTags);
		if (showTags) {
			EditorGUILayout.BeginFadeGroup (1);
			selectedTag = EditorGUILayout.TagField ("Saved Tags", selectedTag);

			if (GUILayout.Button ("Add Tag"))
				_quickSetParent.ExternalAddTag (selectedTag);
			if (GUILayout.Button ("Remove Tag"))
				_quickSetParent.ExternalRemoveTag (selectedTag);
			if (GUILayout.Button ("Remove Last"))
				_quickSetParent.ExternalRemoveLast ();

			EditorGUILayout.EndFadeGroup ();
		}

		EditorGUILayout.Space();
		EditorGUILayout.Space();
		DrawDefaultInspector ();


		if (GUI.changed)
		{
			serializedObject.ApplyModifiedProperties ();
		}
	}

	void OpenUserGuide()
	{
		System.Diagnostics.Process.Start (Application.dataPath + "/QuickScripts/QuickScriptsUserGuide.pdf");
	}
}
