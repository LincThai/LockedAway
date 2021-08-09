//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{
    
    // This class has an Editor script which overrides OnInspectorGUI(). See QSEditor_QuickMover.
    [AddComponentMenu("Quick Scripts/Quick Mover")]
    public class QuickMover : MonoBehaviour
    {
        
        public enum moverBehaviourTypes { forward,reverse,pingPong};
        public moverBehaviourTypes moverBehaviour;
        public bool looping;
        public bool rotate = true;

        public float moveSpeed = 10f; // User facing, should ONLY change if the user changes (or if overridden by a node)
        private float storedSpeed; // Speed managed by class, to allow changes of speed
      //  [HideInInspector]
        public float speedMultiplier
        {
            get { return _speedMultiplier; }
            set { _speedMultiplier = Mathf.Clamp(value, 0f, 1f); }
        }
            private float _speedMultiplier;

        private float speedCurveAdjustor = 1;

        // Reads these from the current track section
        private float easeInT;
        private float easeOutT;
        private float stepsTaken;
        private float initialTimeToCompleteSection;
        

        public QuickTrack currentTrack;
        public QS_TrackSection startTrackSection;
        private Vector3 _startPos;

        [HideInInspector]
        public bool drawGizmos;

        private bool coroutineAllowed;
        private QS_TrackSection currentTrackSection;
        [HideInInspector]
        public int sectionToMoveAlong;
        private float fullTrackLength;
        //private float fullTrackTime;

        private bool reverseDirection;
        public bool canMove = true;

        Coroutine movementCoroutine;
        
        private void OnDrawGizmos()
        {
            if (startTrackSection != null) currentTrack = startTrackSection.parentTrack;
        }


        public void SnapToSectionStart(QS_TrackSection trackSection)
        {
            startTrackSection = trackSection;
            _startPos = trackSection.startNode.transform.position;
            this.transform.position = _startPos;
            this.transform.rotation = startTrackSection.startNode.transform.rotation;
        }

        public void SnapToAssignedStartSection()
        {
            if (startTrackSection != null)
                SnapToSectionStart(startTrackSection);
        }

        public void SnapToTrackStart()
        {
            SnapToSectionStart(currentTrack.trackSections[0]);
        }

        public void SnapToTrackEnd(QS_TrackSection lastTrackSection)
        {
            startTrackSection = lastTrackSection;
            _startPos = startTrackSection.endNode.transform.position;
            this.transform.position = _startPos;
            this.transform.rotation = startTrackSection.endNode.transform.rotation;
        }


        #region Class Behaviour

        private void Start()
        {
            coroutineAllowed = true;
            storedSpeed = moveSpeed;
            if (currentTrack == null) currentTrack = startTrackSection.parentTrack;
            speedMultiplier = currentTrack.trackSections[0].startNode.easeOut ?  0.1f : 1f;

            foreach (QS_TrackSection section in currentTrack.trackSections)
                fullTrackLength += section.GetSectionLength();

            //fullTrackTime = fullTrackLength / storedSpeed;

            sectionToMoveAlong = currentTrack.trackSections.IndexOf(startTrackSection);

            if (moverBehaviour == moverBehaviourTypes.reverse)
            {
                sectionToMoveAlong -= 1;

                if (sectionToMoveAlong < 0)
                sectionToMoveAlong = 0;
            }

            if (currentTrack.trackSections[0].startNode.nodeWaitType == QS_TrackNode.waitType.wait)
                CheckWaitOrStop(currentTrack.trackSections[0].startNode);
           
        }

        public void Update()
        {
            if (!canMove) return;
            
            // Update check for Moving Along Track
            switch (moverBehaviour)
            {
                case moverBehaviourTypes.forward:
                    reverseDirection = false;

                    if (!looping && IsAtTrackEnd() == false)
                        AttemptMoveOnSection();

                    if (looping)
                    {
                        if (IsAtTrackEnd())
                            sectionToMoveAlong = 0;

                        AttemptMoveOnSection();
                    }

                    break;
                case moverBehaviourTypes.reverse:
                    reverseDirection = true;

                    if (!looping && IsAtTrackStart() == false)
                        AttemptMoveOnSection();

                    if (looping)
                    {
                        if (IsAtTrackStart())
                        sectionToMoveAlong = currentTrack.trackSections.Count - 1;

                        AttemptMoveOnSection();
                    }

                    break;
                case moverBehaviourTypes.pingPong:
                    // Ping Pong cannot loop. Illegal! $1000 fine.

                    if (!reverseDirection && IsAtTrackEnd())
                    {
                        reverseDirection = true;
                        sectionToMoveAlong = currentTrack.trackSections.Count - 1;
                    }
                    else if (reverseDirection && IsAtTrackStart())
                    {
                        reverseDirection = false;
                        sectionToMoveAlong = 0;
                    }

                    AttemptMoveOnSection();
                    
                    break;
            }
          
        }

        public void AttemptMoveOnSection()
        {
            if (coroutineAllowed)
            {
                if (currentTrack.trackType == QuickTrack.moverTrackTypes.complex)
                    movementCoroutine = StartCoroutine(MoveAlongSection_Curve(sectionToMoveAlong));
                else
                    movementCoroutine = StartCoroutine(MoveAlongSection_Simple(sectionToMoveAlong));
            }
        }

        private IEnumerator MoveAlongSection_Curve(int sectionNumber)
        {
            coroutineAllowed = false;

            currentTrackSection = currentTrack.trackSections[sectionNumber];
            Transform handle = currentTrackSection.curveHandle.transform;

            Transform startNode;
            Transform endNode;

            if (reverseDirection)
            {
                endNode = currentTrackSection.startNode.transform;
                startNode = currentTrackSection.endNode.transform;
            }
            else
            {
                startNode = currentTrackSection.startNode.transform;
                endNode = currentTrackSection.endNode.transform;
            }

            storedSpeed = moveSpeed;
            speedMultiplier = 1;
            stepsTaken = 0;

            easeInT = 0;
            easeOutT = 0;


            Vector3 moverPos = this.transform.position;
            Quaternion moverRot = this.transform.rotation;

            float sectionLength = currentTrackSection.GetSectionLength();
            initialTimeToCompleteSection = sectionLength / moveSpeed;
          //  float sectionTime = sectionLength / storedSpeed;
            float speedRamp = 0; // 0 - 1. 1 is immediate For gradually changing speed, but NOT braking
            float t = 0;
            speedCurveAdjustor = 1f;

            // THE IMPORTANT BIT. This is the code that moves the object. 
            while (t < 1)
            {
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPaused)
                    yield return null;
#endif

                // same as Time.DT / sectionTime but important for changing speed
                float step = Time.deltaTime / (sectionLength / storedSpeed);

                Vector3 forecastPos;
                if (t + step > 1)
                {
                    QS_TrackSection nextSection = currentTrack.GetNextTrackSection(currentTrackSection, reverseDirection);
                    forecastPos = GetVector3OnCurve(nextSection.endNode.transform.position, 
                        nextSection.curveHandle.transform.position, nextSection.startNode.transform.position, t + step);
                        }
                else
                    forecastPos = GetVector3OnCurve(endNode.position, handle.position, startNode.position, t + step);

                t += step * speedMultiplier * speedCurveAdjustor * Time.timeScale;
                CheckSpeedRamp(sectionNumber, speedRamp, t, sectionLength, step);


                moverPos = GetVector3OnCurve(endNode.position, handle.position, startNode.position, t);
                
                // Check speed adjustments for constant speed.
                float targetStep = (storedSpeed * speedMultiplier) * Time.deltaTime;
                float curveMagnitude = Vector3.Magnitude(forecastPos - moverPos);
                //prevMagnitude = curveMagnitude;

                if (speedMultiplier > 0.9f)
                {
                    if (curveMagnitude < targetStep)
                        speedCurveAdjustor *= 1.1f;
                    else if (curveMagnitude > targetStep)
                        speedCurveAdjustor *= 0.9f;
                }
                else
                    speedCurveAdjustor = 1f;
                

                if (rotate) moverRot = Quaternion.Slerp(startNode.rotation, endNode.rotation, t);

                transform.position = moverPos; // Safer way to manage positions and calculations
                transform.rotation = moverRot;

                //prevStep = step;


                if (t >= 1)
                    break;

                yield return new WaitForEndOfFrame();


            }
            t = 0f;
            coroutineAllowed = true;

            PostMoveChecks();
            yield return null;
        }

        /// <summary>
        /// Return a Vector3 at point T on a Quadratic Bezier Curve
        /// </summary>
        /// <param name="p1">End Node / Destination Position</param>
        /// <param name="p2">Handle Position</param>
        /// <param name="p3">Start Node / Start Position</param>
        /// <param name="t"></param>
        /// <returns></returns>
        Vector3 GetVector3OnCurve(Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            return Mathf.Pow(1 * t, 2) * p1 +
                    p2 * 2 * t * (1 - t) +
                    Mathf.Pow((1 - t), 2) * p3;
        }

        private IEnumerator MoveAlongSection_Simple(int sectionNumber)
        {
            coroutineAllowed = false;

            currentTrackSection = currentTrack.trackSections[sectionNumber];

            Transform startNode;
            Transform endNode;

            if (reverseDirection)
            {
                startNode = currentTrackSection.endNode.transform;
                endNode = currentTrackSection.startNode.transform;
            }
            else
            {
                startNode = currentTrackSection.startNode.transform;
                endNode = currentTrackSection.endNode.transform;
            }
            
            storedSpeed = moveSpeed;
           // speedMultiplier = 1;

            Vector3 moverPos = this.transform.position;
            Quaternion moverRot = this.transform.rotation;

            float sectionLength = Vector3.Distance(currentTrackSection.startNode.transform.position, 
                currentTrackSection.endNode.transform.position);
           // float sectionTime = sectionLength / storedSpeed;
            float speedRamp = 0; // 0 - 1. 1 is immediate For gradually changing speed, but NOT braking
            float t = 0;

            // IMPORTANT. This is the bit that does the actual moving. 
            while (t < 1)
            {

#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPaused)
                    yield return null;
#endif
                // same as Time.DT / sectionTime but important for changing speed
                float step = Time.deltaTime / (sectionLength / storedSpeed);
                CheckSpeedRamp(sectionNumber, speedRamp, t, sectionLength, step);

                t += step * speedMultiplier * Time.timeScale;

                moverPos = Vector3.Lerp(startNode.position, endNode.position, t);
                

                if (rotate) moverRot = Quaternion.Slerp(startNode.rotation, endNode.rotation, t);

                transform.position = moverPos; // Safer way to manage positions and calculations
                transform.rotation = moverRot;

                if (t >= 1)
                    break;

                yield return new WaitForEndOfFrame();
            }

            t = 0f;
            coroutineAllowed = true;

            PostMoveChecks();

            yield return null;
        }

        void PostMoveChecks()
        {
            //Check any Speed Changes

            if (currentTrackSection.overrideMoverSpeed)
            {
                moveSpeed = currentTrackSection.newSpeed;
            }
            storedSpeed = moveSpeed;
            speedMultiplier = 1f;

            // If reached the end of the whole track:

            //Ping Pong Behaviour
            if (moverBehaviour == moverBehaviourTypes.pingPong)
            {
                if (!reverseDirection)
                {
                    sectionToMoveAlong += 1;
                }
                else if (reverseDirection)
                {
                    sectionToMoveAlong -= 1;
                }
            }
            else if (moverBehaviour == moverBehaviourTypes.reverse)
            {
                sectionToMoveAlong -= 1;
            }
            else if (moverBehaviour == moverBehaviourTypes.forward)
            {
                sectionToMoveAlong += 1;
            }

            if (reverseDirection)
            {
                if (sectionToMoveAlong == -1)
                    CheckWaitOrStop(currentTrackSection.startNode);
                else
                    CheckWaitOrStop(currentTrack.trackSections[sectionToMoveAlong].endNode);
            }
            else
                CheckWaitOrStop(currentTrackSection.endNode);

            StopCoroutine(movementCoroutine);

            Update();
        }


        void CheckSpeedRamp(int sectionNumber, float speedRamp, float t, float sectionLength, float step) // Running every frame
        {

            // Check speed ramps
            if (currentTrackSection.overrideMoverSpeed && currentTrackSection.speedChangeSeverity > 0)
            {
                float targetT = Mathf.Lerp(1, 0, currentTrackSection.speedChangeSeverity);

                speedRamp = Mathf.Lerp(moveSpeed, currentTrackSection.newSpeed, (t / targetT));
                storedSpeed = speedRamp;
            }

            // Check if need to ramp speed up / down to STOP
            if (!reverseDirection)
            {
                float pSlow = currentTrackSection.endNode.PercentageOfTrackForEaseIn / 100;
                float _easeInMinSpeed = currentTrackSection.endNode.easeInMinSpeed;
                float _easeOutMinSpeed; // Set below 

                // Check Ease In first
                if (currentTrackSection.endNode.easeIn && t >= 1 - pSlow)
                {
                    // Formula... easeT = (t - (1 - p) ) / p
                    easeInT = (t - (1 - pSlow)) / pSlow;
                    storedSpeed = Mathf.SmoothStep(moveSpeed, _easeInMinSpeed, easeInT);
                }

                // Now check Ease Out... 
                float pRamp = 0;
                if (sectionNumber > 0)
                {
                QS_TrackSection prevSection = currentTrack.trackSections[sectionNumber - 1];

                    if (prevSection.endNode.easeOut)
                    {
                        pRamp = prevSection.endNode.PercentageOfTrackForEaseOut / 100;

                        _easeOutMinSpeed = prevSection.endNode.easeOutMinSpeed;

                        easeOutT = (t / pRamp);
                        storedSpeed = Mathf.SmoothStep(_easeOutMinSpeed, moveSpeed, easeOutT);
                    }

                }
                else if (sectionNumber == 0)
                {
                    // Ramp Out from Start Node
                    if (currentTrackSection.startNode.easeOut)
                    {
                        pRamp = currentTrackSection.startNode.PercentageOfTrackForEaseOut / 100;

                        _easeOutMinSpeed = currentTrackSection.startNode.easeOutMinSpeed;

                        easeOutT = (t / pRamp);
                        storedSpeed = Mathf.SmoothStep(_easeOutMinSpeed, moveSpeed, easeOutT);
                    }
                }
            }
            else // if reversing
            {

                // Check Ramp Up First
                float pRamp = 0;
                if (sectionNumber > 0)
                {

                    if (currentTrackSection.endNode.easeOut)
                    {
                        pRamp = currentTrackSection.endNode.PercentageOfTrackForEaseOut / 100;

                        // Ramp out
                        easeOutT = t / pRamp;
                        speedMultiplier = Mathf.Lerp(0, 1, easeOutT);
                    }

                }
                else if (sectionNumber == 0)
                {
                    // Ramp Out from Start Node
                    if (currentTrackSection.endNode.easeOut)
                    {
                        pRamp = currentTrackSection.endNode.PercentageOfTrackForEaseOut / 100;

                        // Ramp out
                        easeOutT = t / pRamp;
                        speedMultiplier = Mathf.Lerp(0, 1, easeOutT);
                    }
                }



                // Now Check Slow Down
                float pSlow = 0;

                if (sectionNumber == 0)
                {
                    pSlow = currentTrackSection.startNode.PercentageOfTrackForEaseIn / 100;
                    if (currentTrackSection.startNode.easeIn && t >= 1-pSlow)
                    {
                        easeInT = (t - (1 - pSlow)) / pSlow;

                        speedMultiplier = Mathf.Lerp(1, 0.01f,  easeInT);
                    }
                }
                else
                {
                    pSlow = currentTrack.trackSections[sectionNumber - 1].endNode.PercentageOfTrackForEaseIn / 100;
                    if (currentTrack.trackSections[sectionNumber - 1].endNode.easeIn && t >= 1-pSlow)
                    {
                        easeInT = (t - (1 - pSlow)) / pSlow;
                        speedMultiplier = Mathf.Lerp(1, 0.01f,  easeInT);
                    }
                }

            }


        }

        


        bool IsAtTrackEnd()
        {
            return sectionToMoveAlong > currentTrack.trackSections.Count - 1;
        }

        bool IsAtTrackStart()
        {
            return sectionToMoveAlong == -1;
        }

        public void CheckWaitOrStop(QS_TrackNode destinationNode)
        {
            if (coroutineAllowed)
            {
                if (destinationNode.nodeWaitType == QS_TrackNode.waitType.wait && destinationNode.waitTime > 0)
                    StartCoroutine(Wait(destinationNode.waitTime));

                if (destinationNode.nodeWaitType == QS_TrackNode.waitType.stop)
                    StopMoving();
            }

        }

        IEnumerator Wait(float seconds)
        {
            coroutineAllowed = false;
            //storedSpeed = 0;
            yield return new WaitForSecondsRealtime(seconds);
            coroutineAllowed = true;
        }


        IEnumerator EaseIn(float t)
        {
            while (speedMultiplier > 0)
            {
                //storedSpeed = Mathf.Clamp(storedSpeed -= Time.deltaTime * rate, 0, 50);
                speedMultiplier -= Time.deltaTime / t;

                yield return new WaitForEndOfFrame();
            }
            yield return null;
        }

        IEnumerator EaseOut(float t)
        {
            while (speedMultiplier < 1)
            {
                //storedSpeed = Mathf.Clamp(storedSpeed += Time.deltaTime * rate, 0, moveSpeed);
                speedMultiplier += Time.deltaTime / t;

                yield return new WaitForEndOfFrame();
            }
            yield return null;
        }

        private void SetMoving(bool b)
        {
            canMove = b;
        }

        #endregion

        #region Public Events


        public void StartMoving()
        {
           // storedSpeed = moveSpeed;
            SetMoving(true);
        }

        public void StopMoving()
        {
           // storedSpeed = 0;
            SetMoving(false);
        }
        
        public void DeccelerateToStop_InSeconds(float time)
        {
            StartCoroutine(EaseIn(time));
        }

        public void AccelerateToMaxSpeed_InSeconds(float time)
        {
            StartCoroutine(EaseOut(time));
        }

        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }


        public void SetLooping(bool b)
        {
            if (b == true)
            {
                if (moverBehaviour == moverBehaviourTypes.pingPong)
                {
                    if (reverseDirection)
                        moverBehaviour = moverBehaviourTypes.reverse;
                    else
                        moverBehaviour = moverBehaviourTypes.forward;
                }
            }
            looping = b;
        }

        public void MoveForward()
        {
            moverBehaviour = moverBehaviourTypes.forward;
            reverseDirection = false;
        }

        public void MoveReverse()
        {
            moverBehaviour = moverBehaviourTypes.reverse;
            reverseDirection = true;
        }

        public void MovePingPong()
        {
            moverBehaviour = moverBehaviourTypes.pingPong;
            looping = false;
        }

        

       

#endregion
    }
}
