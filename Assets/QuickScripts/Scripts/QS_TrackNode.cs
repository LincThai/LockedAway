//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{
[RequireComponent (typeof (QuickGizmo))]
    public class QS_TrackNode : MonoBehaviour
    {
        [HideInInspector]
        public QS_TrackSection myTrackSection;

        [HideInInspector]
        public GameObject nextNode;
        public enum waitType { pass, wait, stop};
        public waitType nodeWaitType;
        public float waitTime;
        [Space(10)]
        public bool easeIn;
        [Tooltip ("100 = Ease In over full length of track section")]
        [Range(1, 100)]
        public float PercentageOfTrackForEaseIn = 75f;
        [Range (0.01f, 1)]
        public float easeInMinSpeed = 0.1f;
        [Space (10)]
        public bool easeOut;
        [Tooltip("100 = Ease Out over full length of track section")]
        [Range(1, 100)]
        public float PercentageOfTrackForEaseOut = 25f;
        [Range(0.01f, 1)]
        public float easeOutMinSpeed = 0.1f;



        public enum moverNodeType { node, handle };
        [HideInInspector]
        public moverNodeType nodeType;

        [HideInInspector]
        bool drawGizmos;

        private void OnDrawGizmos()
        {
            if (!myTrackSection.parentTrack.drawGizmos)
                return;

            if (easeIn || easeOut || waitTime > 0) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(transform.position, Vector3.one);
                    }
        }
    }
}
