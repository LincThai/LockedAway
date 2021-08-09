//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace QuickScripts
{
	[RequireComponent (typeof(QuickGizmo))]
    [AddComponentMenu("Quick Scripts/Quick Relay")]
        public class QuickRelay : MonoBehaviour {


		private bool running; // Used to stop the coroutine from stacking. 
        public bool runOnStart;
        [Space(10)]
        public float delayInSeconds;
        [Tooltip("How many times this Relay needs to have Run() called before it fires the Events")]
        public int callsUntilRun = 1;
        private int callsReceived;

        [Serializable]
		private class DelayEvent : UnityEvent { }
		[SerializeField]
        [Space (10)]
		private DelayEvent Events = new DelayEvent();

        [Space(10)]
        public bool drawTargetLines = true;
		QuickGizmo quickGiz;

        //public List <Transform> eventTransforms = new List<Transform>();


        void OnDrawGizmos()
		{
            if (callsUntilRun < 1)
                callsUntilRun = 1;

            if (!drawTargetLines)
                return;

            if (quickGiz == null)
                quickGiz = GetComponent<QuickGizmo>();

			int iMax = Events.GetPersistentEventCount ();
            Gizmos.color = quickGiz.gizmoColor;

           

			for (int i = 0; i < iMax; i++) {

                UnityEngine.Object obj = Events.GetPersistentTarget(i);
                GameObject gameObj = obj as GameObject;
                Component component = obj as Component;

                // User drops the scene object in. This is a gameobject
                if (gameObj != null)
                {
                    Gizmos.DrawLine(this.transform.position, gameObj.transform.position);
                }

                // User changes the event target to a function. this is now a component
                else if (component != null)
                {
                    Gizmos.DrawLine(this.transform.position, component.transform.position);
                }
            }
		}

        private void Start()
        {
            if (runOnStart)
                StartCoroutine(RunAfterDelay());
        }


        public void Call()
		{
            callsReceived++;

            if (callsReceived < callsUntilRun)
                return;

			if (running)
				return;
			
			StartCoroutine (RunAfterDelay ());
		}

		IEnumerator RunAfterDelay()
		{
			running = true;
			yield return new WaitForSecondsRealtime (delayInSeconds);
			Events.Invoke ();
            callsReceived = 0;
			running = false;
		}

        public void ForceRun()
        {
            StartCoroutine(RunAfterDelay());
        }

        public void ForceRunImmediate() // Does not reset Calls Received
        {
            Events.Invoke();
        }

        public void ResetCalls()
        {
            callsReceived = 0;
        }

	}
}
