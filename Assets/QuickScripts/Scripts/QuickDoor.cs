//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace QuickScripts
{
    // This class has an Editor script which overrides OnInspectorGUI(). See QSEditor_QuickDoor.
    [AddComponentMenu("Quick Scripts/Quick Door")]
    public class QuickDoor : MonoBehaviour
    {

        public enum doorEnum
        {
            slideUp,
            slideDown,
            slideLeft,
            slideRight,
            slideForward,
            slideBackward,
            pivot,
			twoWayYAxis
        }
        public doorEnum doorType;

        public enum moveEnum
        {
            linear,
            smooth
        }
        public moveEnum moveType;

        public GameObject pivotPoint;
        [Range(-179.9f, 179.9f)]
        public float pivotAmountX;
		[Range(-179.9f, 179.9f)]
        public float pivotAmountY;
		[Range(-179.9f, 179.9f)]
        public float pivotAmountZ;


		// For Two Way Doors
		[Range(0, 179.9f)]
		public float outerPivotAmountY; // Positive values only
		[Range(-0.01f, -179.9f)]
		public float innerPivotAmountY; // Negative values only, max -0.01
		private Quaternion closedRotQ; // Needs to be different to startRotQ
		private Quaternion outerRotQ; 
		private Quaternion innerRotQ;
        // probably could tidy this up but hey I gotta release
		bool twoWayOpeningIn;
		bool twoWayOpeningOut;
        private enum twoWaySide { closed, inside, outside };
        private twoWaySide twoWayCurrentSide; 

        [Space(10)]
		public bool startOpen;
		public enum twoWayStartEnum { 
			startClosed,
			startOpenIn, 
			startOpenOut
		};
		public twoWayStartEnum twoWayStartPos;

        public float duration = 1;
        public bool locked;
		bool readyToUse = true;
		[Tooltip ("Time in seconds before the door can be closed if it has just been opened, and vice versa")]
		public float coolDownTime = 0f;
		[Tooltip ("Close automatically after opening?")]
		public bool autoClose;
		[Tooltip ("How long after opening until the door closes by itself?")]
		public float autoCloseDelay = 0f;

        private MeshRenderer doorMesh;
        public bool customMoveDistance;
        public float moveDistance;
        private float doorWidth;
        private float doorDepth;
        private float doorHeight;

        public AudioClip openStartAudioClip;
        public AudioClip openStopAudioClip;
        public AudioClip closeStartAudioClip;
        public AudioClip closeStopAudioClip;
        public bool extendSoundFX;
        public AudioSource audioSource;

        public bool hasAudio;
        
		bool openDoor;
        Vector3 startPos;
        Vector3 openPos;
        Vector3 startRot;
		Vector3 currentRot;
        float openPercent;
        bool doorIsOpen;
        bool doorIsClosed;

	    //For smooth movement type
		float lerpT; // for getting the lerp T value of the door
		float openAngle;
		Quaternion startRotQ;
		Quaternion endRotQ;

		bool doorMoving;
        bool forceSwap;
        float swapTValue; // the stored Lerp T value when force swapping from Open to Close


        // Events
        [Serializable]
        private class DoorEvents : UnityEvent { }
        [SerializeField]
        [Space(10)]
        private DoorEvents OnOpen = new DoorEvents();
        [SerializeField]
        [Space(10)]
        private DoorEvents OnClose = new DoorEvents();
        public bool hasEvents;
        public bool drawTargetLines = true;
        int openEventCount;
        int closeEventCount;

        void OnDrawGizmos()
        {
            if (!drawTargetLines || !hasEvents)
                return;

            openEventCount = OnOpen.GetPersistentEventCount();
            closeEventCount = OnClose.GetPersistentEventCount();

            Gizmos.color = Color.white;

            // Open Events
            for (int i = 0; i < openEventCount; i++)
            {
                UnityEngine.Object obj = OnOpen.GetPersistentTarget(i);
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

            // Close Events
            for (int i = 0; i < closeEventCount; i++)
            {
                UnityEngine.Object obj = OnClose.GetPersistentTarget(i);
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

            void Start()
        {

			// Generic Set Up

			doorIsClosed = true;
			startPos = transform.localPosition;


            if (hasAudio && audioSource != null)
            {
                audioSource.playOnAwake = false;
            }
            else if (hasAudio && audioSource == null)
            {
				Debug.Log(this.name + " | You must reference an Audio Source if you want to use Audio!", this.gameObject);
            }


			// Set Up for Slide Doors

			if (doorType != doorEnum.pivot && doorType != doorEnum.twoWayYAxis)
            {

				if (GetComponent<MeshRenderer>())
					SetUpMeshDistance();

				if (startOpen)
					StartOpen ();

				return;
            }


			// Set Up for Pivot Doors

			if (doorType == doorEnum.pivot) {

				if (pivotPoint != null) {
					startRot = pivotPoint.transform.localEulerAngles;
				}


				if (startOpen)
					StartOpen ();

				return;
			}


            //For smooth movement type:



			// Set Up for Two Way Doors
			           

			if (doorType == doorEnum.twoWayYAxis) {

				if (pivotPoint != null) {
					startRot = pivotPoint.transform.localEulerAngles;
					//For two-way
					closedRotQ = Quaternion.Euler (startRot);
				}

				SetUpTwoWayRotation ();

				if (twoWayStartPos == twoWayStartEnum.startOpenIn)
					StartOpenIn ();
				if (twoWayStartPos == twoWayStartEnum.startOpenOut)
					StartOpenOut ();
			}


			
        }

		void StartOpen(){
			

			if (doorType == doorEnum.pivot) {
				// Change the starting angle of the pivot
				SetEndRotation();
				pivotPoint.transform.localRotation = endRotQ;
			} else
			{
				// Move door into open slide pos
				transform.localPosition = OpenPosition();
			}

			doorIsOpen = true;
			doorIsClosed = false;
			doorMoving = false;
			readyToUse = true;
			openDoor = true;

		}

		void StartOpenIn() {
			twoWayOpeningIn = true;
            twoWayCurrentSide = twoWaySide.inside;

			SetEndRotation();
			pivotPoint.transform.localRotation = endRotQ;

			doorIsOpen = true;
			doorIsClosed = false;
			doorMoving = false;
			readyToUse = true;
			openDoor = true;

		}

		void StartOpenOut(){
			twoWayOpeningOut = true;
            twoWayCurrentSide = twoWaySide.outside;

			SetEndRotation();
			pivotPoint.transform.localRotation = endRotQ;

			doorIsOpen = true;
			doorIsClosed = false;
			doorMoving = false;
			readyToUse = true;
			openDoor = true;

		}

        void SetUpMeshDistance() // Called from start if Door has a Mesh Component
        {
            doorMesh = GetComponent<MeshRenderer>();
            doorWidth = doorMesh.bounds.size.x;
            doorDepth = doorMesh.bounds.size.z;
            doorHeight = doorMesh.bounds.size.y;
        }

		void SetUpTwoWayRotation() // Called from start
		{
			Vector3 outerRot = new Vector3(startRotQ.x,
				startRotQ.y + outerPivotAmountY,
				startRotQ.z);

			Vector3 innerRot = new Vector3(startRotQ.x,
				startRotQ.y + innerPivotAmountY,
				startRotQ.z);

			outerRotQ = Quaternion.Euler (outerRot);
			innerRotQ = Quaternion.Euler (innerRot);

		}

        void Update()
		{

			if (!this.isActiveAndEnabled)
				return;

			if (doorMoving) {
				openPercent = Vector3.Distance (this.transform.localPosition, startPos) / (Vector3.Distance (openPos, startPos));

			}
		}
        

        
        //public void OpenDoorXPivot(float amount)
        //{
		//	float cappedAmount = Mathf.Clamp(amount, -179.9f, 179.9f);
		//	pivotAmountX = cappedAmount;
        //
		//	if (doorIsOpen) {
		//		startRot = pivotPoint.transform.localEulerAngles;
		//		startRotQ = pivotPoint.transform.localRotation;
		//	}
        //
		//	doorIsOpen = false;
		//	openDoor = false;
        //
		//	OpenDoor();
        //}
        //
        
        //public void OpenDoorYPivot(float amount)
        //{
		//	float cappedAmount = Mathf.Clamp(amount, -179.9f, 179.9f);
        //    pivotAmountY = cappedAmount;
        //
		//	if (doorIsOpen) {
		//		startRot = pivotPoint.transform.localEulerAngles;
		//		startRotQ = pivotPoint.transform.localRotation;
		//	}
        //
		//	doorIsOpen = false;
		//	openDoor = false;
        //
        //    OpenDoor();
        //}
        //
        
        //public void OpenDoorZPivot(float amount)
        //{
		//	float cappedAmount = Mathf.Clamp(amount, -179.9f, 179.9f);
		//	pivotAmountZ = cappedAmount;
        //
		//	if (doorIsOpen) {
		//		startRot = pivotPoint.transform.localEulerAngles;
		//		startRotQ = pivotPoint.transform.localRotation;
		//	}
        //
		//	doorIsOpen = false;
		//	openDoor = false;
        //
		//	OpenDoor();
        //
        //}

		public void OpenTwoWayIn()
		{
            if (locked || twoWayCurrentSide == twoWaySide.outside)
                return;

			if (doorType != doorEnum.twoWayYAxis) {
				Debug.LogWarning (this.name + " | Please use OpenDoor for this door", this.gameObject);
				OpenDoor ();
			}

			if (readyToUse && !twoWayOpeningIn) {


				// Play SFX
				if (hasAudio)
					PlayAudioFX (1);

				twoWayOpeningIn = true;
				twoWayOpeningOut = false;
				openDoor = true;
                twoWayCurrentSide = twoWaySide.inside;

				readyToUse = false;

				if (moveType == moveEnum.smooth)
					StartCoroutine (SmoothPivotOpen ());
				else
					StartCoroutine (LinearPivotOpen ());

			}
		}

		public void OpenTwoWayOut()
		{
            if (locked || twoWayCurrentSide == twoWaySide.inside)
                return;

			if (doorType == doorEnum.pivot) {
				Debug.LogWarning (this.name + " | Please use OpenDoor for this door", this.gameObject);
				OpenDoor ();
			}

			if (readyToUse && !twoWayOpeningOut) {

				// Play SFX
				if (hasAudio)
					PlayAudioFX (1);

				twoWayOpeningIn = false;
				twoWayOpeningOut = true;
				openDoor = true;
                twoWayCurrentSide = twoWaySide.outside;

				readyToUse = false;

				if (moveType == moveEnum.smooth)
					StartCoroutine (SmoothPivotOpen ());
				else
					StartCoroutine (LinearPivotOpen ());
			}
		}

		 void CloseTwoWayDoor() // Called from CloseDoor
		{
			if (readyToUse && openDoor && !doorIsClosed) {


				// Play SFX
				if (hasAudio)
					PlayAudioFX (3);

				readyToUse = false;

				startRotQ = closedRotQ;

				if (moveType == moveEnum.smooth)
					StartCoroutine (SmoothPivotClose ());
				else
					StartCoroutine (LinearPivotClose ());

			}
		}

        /// <summary>
        /// Asks the door to open using the settings in the Inspector.
        /// </summary>
        public void OpenDoor()
		{
            if (locked)
                return;

			if (doorType == doorEnum.twoWayYAxis)
			{
				Debug.LogWarning (this.name + " | A Two-Way type door should only be opened with OpenTwoWayIn or OpenTwoWayOut", this.gameObject);
				OpenTwoWayOut ();
				return;
			}


			// Check if Delayed, not opening, and not already open

			if (readyToUse && !openDoor && !doorIsOpen) {


				// Play SFX
				if (hasAudio)
					PlayAudioFX (1);

				// Set appropriate bools
				openDoor = true;
				doorMoving = true;
				doorIsClosed = false;
				doorIsOpen = false;

				if (doorType != doorEnum.pivot) {
					// For all NON pivot movements (LINEAR)

					if (moveType == moveEnum.smooth) 
						StartCoroutine (SmoothSlideToOpenPosition ());
					 else 
						StartCoroutine (LinearSlideToOpenPosition ());
					
				} 
				else {
					
					// For PIVOT movements
					if (moveType == moveEnum.smooth)
						StartCoroutine (SmoothPivotOpen ());
					else
						StartCoroutine (LinearPivotOpen ());
				}

				// The door must now finish opening.
				readyToUse = false;
			}		
		}

		IEnumerator AutoClose()
		{
			yield return new WaitForSecondsRealtime (autoCloseDelay);
			//Account for door being asked to open opposite direction during the delay

			if (doorIsOpen && !doorMoving) {
				CloseDoor ();
			}
		}


		/// <summary>
		/// Asks the door to close using the settings in the Inspector.
		/// </summary>
		public void CloseDoor()
		{

			if (doorType == doorEnum.twoWayYAxis) {
				CloseTwoWayDoor ();
				return;
			}

			// Check if Delayed, open/opening, and not already closed
			if (readyToUse && openDoor && !doorIsClosed) {


				// Play SFX
				if (hasAudio)
					PlayAudioFX (3);

				// Set appropriate bools
				openDoor = false;
				doorMoving = true;
				doorIsClosed = false;
				doorIsOpen = false;

				if (doorType != doorEnum.pivot) {
					// For all NON pivot movements (SLIDE)

					if (moveType == moveEnum.smooth)
						StartCoroutine (SmoothSlideToClosedPosition ());
					else
						StartCoroutine (LinearSlideToClosedPosition ());
					
				} 
				else {
					// For PIVOT movements
					if (moveType == moveEnum.smooth)
						StartCoroutine (SmoothPivotClose() );
					else
						StartCoroutine (LinearPivotClose ());
				}

				// The door must now finish closing.
				readyToUse = false;
			}
		}

			     

        void DoorIsOpen()
        {
			if (hasAudio)
			{
				audioSource.Stop ();
				PlayAudioFX(2);
			}

            doorIsOpen = true;
            doorIsClosed = false;
			doorMoving = false;
            
            // Events should not fire if being force swapped, as logically hasn't reached its 'Open State'
            if (!forceSwap && hasEvents && OnOpen.GetPersistentEventCount() > 0)
                OnOpen.Invoke();

            if (forceSwap)
            {
                readyToUse = true;
                CloseDoor();
                return;
            }

			StartDelay ();
        }
		               
        
       
        void DoorIsClosed()
		{
			if (hasAudio) {
				audioSource.Stop ();
				PlayAudioFX (4);
			}

			doorIsOpen = false;
			doorIsClosed = true;
			doorMoving = false;

			twoWayOpeningIn = false;
			twoWayOpeningOut = false;


            if (forceSwap)
            {
                readyToUse = true;

                if (twoWayCurrentSide == twoWaySide.inside)
                    OpenTwoWayIn();
                else 
                    OpenTwoWayOut();

                return;
            }
            else
            { twoWayCurrentSide = twoWaySide.closed; }


            // Events should not fire if being force swapped, as logically hasn't reached its 'Closed State'
            if (hasEvents && OnClose.GetPersistentEventCount() > 0)
                OnClose.Invoke();

            StartDelay ();
		}

        /// <summary>
        /// Finds the open position based on the movement direction.
        /// For pivoting doors, use OpenRotation.
        /// </summary>
        /// <returns>The open position as a Vector3.</returns>
        Vector3 OpenPosition()
        {
            switch (doorType)
            {
                case doorEnum.slideUp:
                    if (!customMoveDistance)
                    {
                        moveDistance = doorHeight;
                    }

                    openPos = startPos + (transform.up * moveDistance);
                    break;
                case doorEnum.slideDown:
                    if (!customMoveDistance)
                        moveDistance = doorHeight;

                    openPos = startPos + (transform.up * -moveDistance);
                    break;
                case doorEnum.slideLeft:
                    if (!customMoveDistance)
                        moveDistance = doorWidth;

                    openPos = startPos + (transform.right * -moveDistance);
                    break;
                case doorEnum.slideRight:
                    if (!customMoveDistance)
                        moveDistance = doorWidth;

                    openPos = startPos + (transform.right * moveDistance);
                    break;
                case doorEnum.slideForward:
                    if (!customMoveDistance)
                        moveDistance = doorDepth;

                    openPos = startPos + (transform.forward * moveDistance);
                    break;
                case doorEnum.slideBackward:
                    if (!customMoveDistance)
                        moveDistance = doorDepth;

                    openPos = startPos + (transform.forward * -moveDistance);
                    break;
                case doorEnum.pivot:
				Debug.Log(this.name + " | A pivot cannot use a Vector 3! Use OpenRotation instead", this.gameObject);
                    break;
            }
            return openPos;
        }

        /// <summary>
        /// Finds the open position based on the desired rotation.
        /// For sliding doors, use OpenPosition.
        /// </summary>
        /// <returns>The open position as a Quaternion.</returns>
        void SetEndRotation()
        {

			Vector3 resultEuler = Vector3.zero;

			if (doorType == doorEnum.pivot) {
				if (pivotAmountX < 0 || pivotAmountY < 0 || pivotAmountZ < 0) {
					// If door needs to open on a negative angle, change the start rotation to be -0.1f so the Quaternion lerps correctly on the negative 180
					//Vector3 tempv = Vector3.one * -0.1f;
					startRot = startRot * -0.1f;
				}

				resultEuler = new Vector3 (startRot.x + pivotAmountX,
					startRot.y + pivotAmountY,
					startRot.z + pivotAmountZ);

			} else if (doorType == doorEnum.twoWayYAxis) {

				if (twoWayCurrentSide == twoWaySide.inside)
					resultEuler = innerRotQ.eulerAngles;
				
				if (twoWayCurrentSide == twoWaySide.outside)
					resultEuler = outerRotQ.eulerAngles;
			}

			endRotQ = Quaternion.Euler (resultEuler);
			openAngle = Quaternion.Angle (startRotQ, endRotQ);

        }

		bool ApproximateRotationMatch (Quaternion current, Quaternion target)
		{
			return 1 - Mathf.Abs (Quaternion.Dot (current, target)) < 0.00001f;
		}


        /// <summary>
        /// Plays the Audio Effects.
        /// </summary>
        /// <param name="audioID"><para>1 = Open Start SFX</para> <para> 2 = Open Stop SFX</para> <para> 3 = Close Start SFX</para>  4 = Close Stop SFX</param>
        void PlayAudioFX(int audioID)
        {
			// This functions should never be called continuously.
			// Allow to loop OpenFX and Close FX
			// Allow all sfx to interrupt the others


			if (!hasAudio)
				return;

            if (audioSource == null)
            {
                Debug.LogError(this.name + " | Quick Door Component is trying to play audio but there is no Audio Source!", this.gameObject);
                return;
            }


			switch (audioID) {

			case 1: //Play Opening
				if (extendSoundFX) {
					audioSource.loop = true;
				}

				if (openStartAudioClip != null) {
					audioSource.clip = openStartAudioClip;
					audioSource.Play ();           
				}
				break;

			case 2: // Play Open Stop
				if (openStopAudioClip != null) {
					audioSource.loop = false;
					audioSource.clip = openStopAudioClip;
					audioSource.Play ();           

				}
				break;

			case 3: // Play Closing
				if (extendSoundFX) {
					audioSource.loop = true;
				}
				if (closeStartAudioClip != null) {
					audioSource.clip = closeStartAudioClip;
					audioSource.Play ();           

				}
				break;

			case 4: // Play Close Stop
				if (closeStopAudioClip != null) {
					audioSource.loop = false;
					audioSource.clip = closeStopAudioClip;
					audioSource.Play ();           

				}
				break;
			}
	
        }

		void StartDelay() // Runs AFTER the door has finished opening or closing. This is the cooldown behaviour.
		{
			readyToUse = false;

			if (autoClose && doorIsOpen)
				StartCoroutine (AutoClose ());

			if (coolDownTime == 0)
				readyToUse = true;
			 else
				StartCoroutine (InterruptDelay ());
		}		

		IEnumerator InterruptDelay()
		{
			yield return new WaitForSecondsRealtime (coolDownTime);
			readyToUse = true;
		}


		// There's definitely a smarter way to set out these Coroutines... but this works. 

		IEnumerator SmoothPivotOpen()
		{
			StopCoroutine (SmoothPivotClose ());

			SetEndRotation ();

			bool complete = false;

			float moveSpeed = (openAngle / duration) * (Time.deltaTime * duration);

            lerpT = (forceSwap) ? swapTValue :  0; // if force swapping open/close, use the stored lerp T val
            forceSwap = false;


            while (!complete) {

				// This code does the actual smoothed rotation. 
				lerpT += Time.deltaTime / duration / openAngle;
				pivotPoint.transform.localRotation = Quaternion.Slerp (pivotPoint.transform.localRotation, endRotQ, lerpT * moveSpeed);

				// Check if we are close enough to the target pivot
				Vector3 pivotRot = pivotPoint.transform.localEulerAngles;
				complete = ApproximateRotationMatch (Quaternion.Euler (pivotRot), endRotQ);

                if (forceSwap)
                {
                    swapTValue = lerpT;
                    complete = true;
                }

                yield return null;
			}

			DoorIsOpen ();
		}

		IEnumerator SmoothPivotClose()
		{
			StopCoroutine (SmoothPivotOpen ());

			if (doorType == doorEnum.twoWayYAxis)
				startRotQ = closedRotQ;
			
			SetEndRotation ();

			bool complete = false;

			float moveSpeed = (openAngle / duration) * (Time.deltaTime * duration);

            lerpT = (forceSwap) ? swapTValue : 0; // if force swapping open/close, use the stored lerp T val
            forceSwap = false;


            // TO DO: May need to change this to subtract LerpT from 1

            while (!complete) {

				lerpT += Time.deltaTime / duration / openAngle;

				pivotPoint.transform.localRotation = Quaternion.Slerp (pivotPoint.transform.localRotation, startRotQ, lerpT * moveSpeed);

				// Check if we are close enough to the target pivot
				Vector3 pivotRot = pivotPoint.transform.localEulerAngles;
				complete = ApproximateRotationMatch (Quaternion.Euler (pivotRot), startRotQ);

                if (forceSwap)
                {
                    swapTValue = lerpT;
                    complete = true;
                }

                yield return null;
			}

			DoorIsClosed ();
		}

		IEnumerator LinearPivotOpen()
		{
			StopCoroutine (LinearPivotClose ());

			SetEndRotation ();
			Vector3 endRotEuler = endRotQ.eulerAngles;

            if (doorType == doorEnum.pivot)
            {
                // Lock unused values to 0
                if (pivotAmountX == 0)
                    endRotEuler.x = 0;
                if (pivotAmountY == 0)
                    endRotEuler.y = 0;
                if (pivotAmountZ == 0)
                    endRotEuler.z = 0;
            }

            lerpT = (forceSwap) ? 1 - swapTValue : 0; // if force swapping open/close, use the stored lerp T val

            forceSwap = false;


            while (lerpT < 1) {

				lerpT = Mathf.Clamp (lerpT += Time.deltaTime / duration, 0, 1);
                // Lock unused values to 0
               

				pivotPoint.transform.localRotation = Quaternion.Euler (
					Mathf.LerpAngle (startRot.x, endRotEuler.x, lerpT),
					Mathf.LerpAngle (startRot.y, endRotEuler.y, lerpT),
					Mathf.LerpAngle (startRot.z, endRotEuler.z, lerpT));

                if (forceSwap)
                {
                    swapTValue = lerpT;
                    lerpT = 1;
                }

                yield return null;
			}

			DoorIsOpen ();

		}

		IEnumerator LinearPivotClose()
		{
			StopCoroutine (LinearPivotOpen ());

			if (doorType == doorEnum.twoWayYAxis)
				startRotQ = closedRotQ;
			
			SetEndRotation ();
			Vector3 endRotEuler = endRotQ.eulerAngles;

            if (doorType == doorEnum.pivot)
            {
                // Lock unused values to 0
                if (pivotAmountX == 0)
                    endRotEuler.x = 0;
                if (pivotAmountY == 0)
                    endRotEuler.y = 0;
                if (pivotAmountZ == 0)
                    endRotEuler.z = 0;
            }


            lerpT = (forceSwap) ? 1 - swapTValue : 0; // if force swapping open/close, use the stored lerp T val

            forceSwap = false;


            while (lerpT < 1) {

				lerpT = Mathf.Clamp (lerpT += Time.deltaTime / duration, 0, 1);

				pivotPoint.transform.localRotation = Quaternion.Euler(
					Mathf.LerpAngle(endRotEuler.x, startRot.x, lerpT),
					Mathf.LerpAngle(endRotEuler.y, startRot.y, lerpT),
					Mathf.LerpAngle(endRotEuler.z, startRot.z, lerpT));

                if (forceSwap)
                {
                    swapTValue = lerpT;
                    lerpT = 1;
                }

                yield return null;
			}
			DoorIsClosed ();

		}

		IEnumerator LinearSlideToOpenPosition()
		{
			StopCoroutine (LinearSlideToClosedPosition ());
			openPos = OpenPosition ();


            lerpT = (forceSwap) ? 1 - swapTValue : 0; // if force swapping open/close, use the stored lerp T val
            forceSwap = false;

            while (lerpT < 1) {

				lerpT = Mathf.Clamp (lerpT += Time.deltaTime / duration, 0, 1);
				transform.localPosition = Vector3.Lerp (startPos, openPos, lerpT);

                if (forceSwap)
                {
                    swapTValue = lerpT;
                    lerpT = 1;
                }

                yield return null;
			}

			DoorIsOpen ();
		}

		IEnumerator LinearSlideToClosedPosition()
		{
			StopCoroutine (LinearSlideToOpenPosition ());


            lerpT = (forceSwap) ? 1 - swapTValue : 0; // if force swapping open/close, use the stored lerp T val
            forceSwap = false;

            while (lerpT < 1) {

				lerpT = Mathf.Clamp (lerpT += Time.deltaTime / duration, 0, 1);
				transform.localPosition = Vector3.Lerp (openPos, startPos, lerpT);

                if (forceSwap)
                {
                    swapTValue = lerpT;
                    lerpT = 1;
                }

                yield return null;
			}

			DoorIsClosed ();

		}

		IEnumerator SmoothSlideToOpenPosition ()
		{
			StopCoroutine (SmoothSlideToClosedPosition());
			Vector3 targetPos = OpenPosition ();
			float speed = moveDistance / (duration + 2);
            lerpT = (forceSwap) ? swapTValue : 0; // if force swapping open/close, use the stored lerp T val
            forceSwap = false;


            while (lerpT < 1) {
				Vector3 currentPos = transform.localPosition;
				lerpT += Time.deltaTime / duration / moveDistance;
				transform.localPosition = Vector3.Lerp (currentPos, targetPos, Mathf.SmoothStep(0.0f, 1.0f, lerpT * speed));


				if (openPercent > 0.999f) {
					lerpT = 1;
				}

                if (forceSwap)
                {
                    swapTValue = lerpT;
                    lerpT = 1;
                }

                yield return null;
			}

			DoorIsOpen ();
		}

		IEnumerator SmoothSlideToClosedPosition ()
		{
			StopCoroutine (SmoothSlideToOpenPosition());
            lerpT = (forceSwap) ? swapTValue : 0; // if force swapping open/close, use the stored lerp T val
            forceSwap = false;

            float speed = moveDistance / (duration + 2); 

			while (lerpT < 1) {
				Vector3 currentPos = transform.localPosition;
				lerpT += Time.deltaTime / duration / moveDistance;
				transform.localPosition = Vector3.Lerp (currentPos, startPos, Mathf.SmoothStep (0.0f, 1.0f, lerpT * speed));


				if (openPercent < 0.001f) {
					lerpT = 1;
				}

                if (forceSwap)
                {
                    swapTValue = lerpT;
                    lerpT = 1;
                }

                yield return null;
			}

			DoorIsClosed ();
		}


        #region Public Functions
        public void SwapOpenClose()
        {

			if (doorIsOpen)
				CloseDoor ();
			else
				OpenDoor ();

        }
        public void SwapOpenCloseImmediately()
        {
            if (readyToUse)
            {
                if (doorIsClosed)
                    OpenDoor();
                else
                    CloseDoor();
            }
            else
            {
                forceSwap = true;
            }

        }
        public void SetDuration(float t)
        {
            duration = t;
        }
        public void SetLocked(bool b)
        {
            locked = b;
        }
        public void SetHasAudio(bool b)
        {
            hasAudio = b;
        }
        public void SetCustomMoveDistance(float x)
        {
            customMoveDistance = true;
            moveDistance = x;
        }
        #endregion
    }
}
