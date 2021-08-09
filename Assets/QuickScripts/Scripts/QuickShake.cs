//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{

    [AddComponentMenu("Quick Scripts/Quick Shake")]
    public class QuickShake : MonoBehaviour
    {

        Vector3 originPos;
        Vector3 newPos;
        public bool isShaking = true;

        public float intensityX;
        public float intensityY;
        public float intensityZ;

        bool easingIn;
        [Space(10)]
        public float easeInTime;
        bool easingOut;
        public float easeOutTime;

        float storedIntensityX;
        float storedIntensityY;
        float storedIntensityZ;

        float shakeX;
        float shakeY;
        float shakeZ;

        bool pulsing;


        // Use this for initialization
        void Start()
        {
            originPos = this.transform.localPosition;
            storedIntensityX = intensityX;
            storedIntensityY = intensityY;
            storedIntensityZ = intensityZ;
        }

        // Update is called once per frame
        void Update()
        {
            if (!isShaking)
                return;

            shakeX = Random.Range(-storedIntensityX, storedIntensityX) * Time.deltaTime;
            shakeY = Random.Range(-storedIntensityY, storedIntensityY) * Time.deltaTime;
            shakeZ = Random.Range(-storedIntensityZ, storedIntensityZ) * Time.deltaTime;

            newPos = originPos + new Vector3(shakeX, shakeY, shakeZ);

            this.transform.localPosition = newPos;

        }

        IEnumerator DoEaseIn()
        {
            easingIn = true;
            float t = 0;

            isShaking = true;

            while (t < easeInTime)
            {
                t += Time.deltaTime;

                storedIntensityX = Mathf.SmoothStep(0, intensityX, t);
                storedIntensityY = Mathf.SmoothStep(0, intensityY, t);
                storedIntensityZ = Mathf.SmoothStep(0, intensityZ, t);

                yield return new WaitForEndOfFrame();
            }
            easingIn = false;

            if (pulsing)
                StartCoroutine(DoEaseOut());

            yield return null;
        }

        IEnumerator DoEaseOut()
        {
            easingOut = true;
            
            float t = easeOutTime;

            while (t > 0)
            {
                t -= Time.deltaTime;

                storedIntensityX = Mathf.SmoothStep(0, intensityX, t);
                storedIntensityY = Mathf.SmoothStep(0, intensityY, t);
                storedIntensityZ = Mathf.SmoothStep(0, intensityZ, t);

                yield return new WaitForEndOfFrame();
            }
            easingOut = false;
            isShaking = false;

            pulsing = false;
            yield return null;
        }

        public void EaseIn()
        {
            if (!easingIn || !easingOut)
            StartCoroutine(DoEaseIn());
        }

        public void EaseOut()
        {
            if (!easingOut || !easingOut)
                StartCoroutine(DoEaseOut());
        }

        public void EaseInAndOut()
        {
            if (pulsing)
                return;

            pulsing = true;
            EaseIn();
        }
    }

}
