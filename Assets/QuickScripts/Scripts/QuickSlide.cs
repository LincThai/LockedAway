//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{
    [AddComponentMenu("Quick Scripts/Quick Slide")]
    public class QuickSlide : MonoBehaviour
    {

        public bool slideOnStart;
        [Header("Start Position")]
        public Vector3 startPos;
        public Quaternion startRot;
        [Header("End Position")]
        public Vector3 endPos;
        public Quaternion endRot;
        [Space(10)]
        public float duration = 1;
        Vector3 beginPos;
        Quaternion beginRot;
        Vector3 destinationPos;
        Quaternion destinationRot;
        bool moving;
        int pos; // 0 = start, 1 = end

        // Use this for initialization
        void Start()
        {
            this.transform.position = startPos;
            this.transform.rotation = startRot;

            if (slideOnStart)
                SlideToEnd();
        }

        IEnumerator DoMove()
        {
            float t = 0;

            while (t < 1)
            {
                t += Time.deltaTime / duration;

                this.transform.position = Vector3.Lerp(beginPos, destinationPos, t);
                this.transform.rotation = Quaternion.Slerp(beginRot, destinationRot, t);

                yield return new WaitForEndOfFrame();
            }
            moving = false;
            yield return null;
        }

        public void SlideToEnd()
        {
            if (moving) return;

            beginPos = startPos;
            beginRot = startRot;

            destinationPos = endPos;
            destinationRot = endRot;

            moving = true;

            StartCoroutine(DoMove());
        }


        public void SlideToStart()
        {
            if (moving) return;

            beginPos = endPos;
            beginRot = endRot;

            destinationPos = startPos;
            destinationRot = startRot;

            moving = true;

            StartCoroutine(DoMove());
        }

        public void SetStartPosition(Vector3 newPos)
        {
            startPos = newPos;
        }

        public void SetStartRotation(Quaternion newRot)
        {
            startRot = newRot;
        }

        public void SetEndPosition(Vector3 newPos)
        {
            endPos = newPos;
        }

        public void SetEndRotation(Quaternion newRot)
        {
            endRot = newRot;
        }

        public void SetDuration(float f)
        {
            duration = f;
        }
    }

}