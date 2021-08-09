//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{

    [AddComponentMenu("Quick Scripts/Quick Pendulum")]
    public class QuickPendulum : MonoBehaviour
    {

        public float angleX = 90;
        float storedAngleX;
        float initialAngleX;

        public float angleY = 0;
        float storedAngleY;
        float initialAngleY;

        public float angleZ = 0;
        float storedAngleZ;
        float initialAngleZ;

        public float speed = 1;
        [Range(0, 3)]
        public float offset = 0;
        public bool isSwinging = true;

        private Vector3 startRotation;
        private float internalTime;
        private float targetSpeed;
        Coroutine activeCoroutine;

        void Start()
        {
            storedAngleX = angleX;
            storedAngleY = angleY;
            storedAngleZ = angleZ;
            startRotation = transform.eulerAngles;
        }

        void Update()
        {
            if (angleX != 0)
                startRotation.x = 1;
            if (angleY != 0)
                startRotation.y = 1;
            if (angleZ != 0)
                startRotation.z = 1;
        }

        void FixedUpdate()
        {
            if (!isSwinging)
                return;


            internalTime += Time.fixedDeltaTime * Time.timeScale;
            float t = (Mathf.Sin(offset + internalTime * speed));
            transform.rotation = Quaternion.Euler(startRotation.x * angleX * t, startRotation.y * angleY * t, startRotation.z * storedAngleZ * t);
        }

        IEnumerator Decelerate(float time)
        {
            initialAngleX = storedAngleX;
            initialAngleY = storedAngleY;
            initialAngleZ = storedAngleZ;
            
            float t = 0;
            while (t < 1)
            {
                storedAngleX = Mathf.Lerp(initialAngleX, 0, t);
                storedAngleY = Mathf.Lerp(initialAngleY, 0, t);
                storedAngleZ = Mathf.Lerp(initialAngleZ, 0, t);

                t += Time.fixedDeltaTime / time * Time.timeScale;
                yield return new WaitForEndOfFrame();
            }

            yield return null;
        }

        IEnumerator Accelerate(float time)
        {
            initialAngleX = storedAngleX;
            initialAngleY = storedAngleY;
            initialAngleZ = storedAngleZ;

            float t = 0;
            while (t < 1)
            {
                storedAngleX = Mathf.Lerp(initialAngleX, angleX, t);
                storedAngleY = Mathf.Lerp(initialAngleY, angleY, t);
                storedAngleZ = Mathf.Lerp(initialAngleZ, angleZ, t);

                t += Time.fixedDeltaTime / time * Time.timeScale;
                yield return new WaitForEndOfFrame();
            }

            yield return null;
        }

        #region Public Events



        public void SetSwinging(bool active)
        {
            isSwinging = active;
        }

        public void SetAngleX(float x)
        {
            angleX = x;
        }
        public void SetAngleY(float y)
        {
            angleY = y;
        }
        public void SetAngleZ(float z)
        {
            angleZ = z;
        }
        public void SetSpeed(float t)
        {
            speed = t;
        }
        public void DecelerateOverTime(float time)
        {
            if (activeCoroutine != null) StopCoroutine(activeCoroutine);
            activeCoroutine = StartCoroutine(Decelerate(time));
        }
        public void AccelerateOverTime(float time)
        {
            if (activeCoroutine != null) StopCoroutine(activeCoroutine);
            activeCoroutine = StartCoroutine(Accelerate(time));
        }

        public void SetOffset(float o)
        {
            offset = Mathf.Clamp(o, 0, 3);
        }

        #endregion
    }
}
