//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{

    [AddComponentMenu("Quick Scripts/Quick Track")]
    public class QuickTrack : MonoBehaviour
    {
        // General stuff
        public bool drawGizmos = true;
        public Color gizmoColor = Color.white;

        // What kind of track is it?
        public enum moverTrackTypes { simple, complex };
        public moverTrackTypes trackType;

        // List of all Track Sections
        public List<QS_TrackSection> trackSections = new List<QS_TrackSection>();
        public QS_TrackSection LastTrackSection()
        {
            return trackSections[trackSections.Count - 1];
        }

        // List of nodes? Might not be necessary

        // Public functions to tell the QuickMover what track section is next

        public QS_TrackSection GetNextTrackSection(QS_TrackSection currentSection, bool isReversing)
        {
            int index = trackSections.IndexOf(currentSection);

            index = Mathf.Clamp((index = isReversing ? index -= 1 : index += 1), 0, trackSections.Count - 1);
            return trackSections[index];
        }

        #region Editor
#if UNITY_EDITOR
        public void AddTrackSection()
        {



            if (trackSections.Count == 0 || trackSections[0] == null)
                trackSections.Clear();

            // Create the track section
            GameObject trackSection = new GameObject(this.name + " Track Section " + trackSections.Count);
            trackSection.transform.parent = this.transform;
            trackSection.transform.SetAsLastSibling();
            

            // Select the prefab and update the components
            UnityEditor.Selection.activeObject = trackSection;


            // Move the track section to the Mover's position
            var sceneView = UnityEditor.SceneView.lastActiveSceneView;
            if (sceneView != null && trackSections.Count == 0)
            {
                // trackSection.transform.position = sceneView.camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
                trackSection.transform.position = this.transform.position;
            }

            // Now the track section is in position, ask it to create it's components
            trackSection.AddComponent<QS_TrackSection>().SetUpSelf(this, trackSections.Count, false);
            trackSections.Add(trackSection.GetComponent<QS_TrackSection>());


            // Make the new End Node the next active selection so it is easier to make extensive tracks
            UnityEditor.Selection.activeObject = trackSection.GetComponent<QS_TrackSection>().endNode.gameObject;


        }

        public void InsertTrack(QS_TrackSection currentTrackSection)
        {
            if (trackSections.Count > 0)
            {
                int indexOfCurrent = trackSections.IndexOf(currentTrackSection);

                if (indexOfCurrent == trackSections.Count - 1) // If last track section, run Add Track instead
                {
                    AddTrackSection();
                    return;
                }

                // Create the track section
                GameObject newTrackSection = new GameObject(this.name + " Track Section " + trackSections.Count);
                newTrackSection.transform.parent = this.transform;



                newTrackSection.transform.SetSiblingIndex(indexOfCurrent + 1);
                //indexOfCurrent++; // Otherwise index of current is now index of New Section!
                // Check if this is the start pos node

                // Select the prefab and update the components
                UnityEditor.Selection.activeObject = newTrackSection;

                // Move the track section to its position
                Vector3 midwayVector = (currentTrackSection.endNode.transform.position +
                    trackSections[indexOfCurrent + 1].endNode.transform.position) / 2;

                newTrackSection.transform.position = midwayVector;

                // Re-align next track parts
                trackSections[indexOfCurrent + 1].startNode.transform.position = newTrackSection.transform.position;

                // Now the track section is in position, ask it to create it's components
                newTrackSection.AddComponent<QS_TrackSection>().SetUpSelf(this,indexOfCurrent + 1, true);

                trackSections.Insert(indexOfCurrent + 1, newTrackSection.GetComponent<QS_TrackSection>());
                foreach (QS_TrackSection ts in trackSections)
                {
                    ts.orderNumber = trackSections.IndexOf(ts);
                    ts.gameObject.name = this.name + " Track Section " + ts.orderNumber;
                }

                // Make the new End Node the next active selection so it is easier to make extensive tracks
                UnityEditor.Selection.activeObject = newTrackSection.GetComponent<QS_TrackSection>().endNode.gameObject;


            }
        }

        public void DeleteThisTrack(QS_TrackSection trackToDelete)
        {

            int index = trackSections.IndexOf(trackToDelete);
            trackSections.Remove(trackToDelete);

            foreach (QS_TrackSection ts in trackSections)
            {
                ts.orderNumber = trackSections.IndexOf(ts);
                ts.gameObject.name = this.name + " Track Section " + ts.orderNumber;
            }


            // If track is start track and not the only track
            if (index == 0 && trackSections[index + 1] != null)
            {

                GameObject newStartNode = trackSections[0].startNode.gameObject;

                newStartNode.SetActive(true);
                newStartNode.name = "Start Node";


                UnityEditor.Selection.activeGameObject = newStartNode;


            }

            // If track is end track
            else if (index == trackSections.Count)
            {
                UnityEditor.Selection.activeGameObject = trackSections[index - 1].endNode.gameObject;
            }

            // If track is a mid track
            else if (index > 0)
            {
                trackSections[index].startNode.transform.position = trackSections[index].endNode.transform.position;
                trackSections[index].startNode.transform.rotation = trackSections[index].endNode.transform.rotation;
                UnityEditor.Selection.activeGameObject = trackSections[index - 1].endNode.gameObject;
            }

            DestroyImmediate(trackToDelete.gameObject);



        }

        public void DeleteLastTrack()
        {
            if (trackSections.Count == 0)
                return;

            QS_TrackSection lastSection = trackSections[trackSections.Count - 1];
            trackSections.Remove(lastSection);
            DestroyImmediate(lastSection.gameObject);
        }

        public void PointNodeAtHandle(QS_TrackNode thisNode, QS_TrackSection currentTrackSection)
        {
            if (thisNode == currentTrackSection.startNode)
                thisNode.transform.LookAt(currentTrackSection.curveHandle.transform);
            else if (thisNode == currentTrackSection.endNode)
            {
                int index = trackSections.IndexOf(currentTrackSection);

                if (index < trackSections.Count)
                    thisNode.transform.LookAt(trackSections[index + 1].curveHandle.transform);
            }
        }

        public void PointNodeAtNode(QS_TrackNode thisNode, QS_TrackSection currentTrackSection)
        {
            if (thisNode == currentTrackSection.startNode)
                thisNode.transform.LookAt(currentTrackSection.endNode.transform);
            else if (thisNode == currentTrackSection.endNode)
            {
                int index = trackSections.IndexOf(currentTrackSection);

                if (index < trackSections.Count)
                    thisNode.transform.LookAt(trackSections[index + 1].endNode.transform);
            }
        }


        public void PointAllNodesAtNodes()
        {

            for (int i = 0; i < trackSections.Count; i++)
            {
                if (i + 1 < trackSections.Count)
                {
                    trackSections[i].startNode.transform.LookAt(trackSections[i].endNode.transform);
                    trackSections[i].endNode.transform.LookAt(trackSections[i + 1].endNode.transform);
                }
            }
        }

        public void PointAllNodesAtHandles()
        {
            for (int i = 0; i < trackSections.Count; i++)
            {
                if (i + 1 < trackSections.Count)
                {
                    trackSections[i].startNode.transform.LookAt(trackSections[i].curveHandle.transform);
                    trackSections[i].endNode.transform.LookAt(trackSections[i + 1].curveHandle.transform);
                }
            }
        }

        public void RefreshNodeList()
        {
            trackSections.Clear();
            trackSections.AddRange(this.GetComponentsInChildren<QS_TrackSection>());
        }

#endif
        #endregion

    }

}