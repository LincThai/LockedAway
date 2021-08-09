//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{

    [AddComponentMenu("Quick Scripts/Quick Scale")]
    public class QuickScale : MonoBehaviour
    {

        public float scaleX;
        public float scaleY;
        public float scaleZ;
        public float speed = 1;
        [Range(0, 3)]
        public float offset;
        Vector3 originalScale;
        private float internalTime;
        public bool isScaling = true;

        void Start()
        {
            originalScale = transform.localScale;
        }

        void FixedUpdate()
        {
            if (!isScaling)
                return;

            internalTime += Time.fixedDeltaTime * Time.timeScale;
            float x = (Mathf.Sin(offset + internalTime * speed));
            transform.localScale = (originalScale + new Vector3(scaleX * x, scaleY * x, scaleZ * x));
        }

        #region Public Events

        public void SetScaling(bool active)
        {
            isScaling = active;
        }

        public void SetScaleX(float x)
        {
            scaleX = x;
        }
        public void SetScaleY(float y)
        {
            scaleY = y;
        }
        public void SetScaleZ(float z)
        {
            scaleZ = z;
        }
        public void SetSpeed(float t)
        {
            speed = t;
        }
        public void SetOffset(float o)
        {
            offset = Mathf.Clamp(o, 0, 3);
        }

        #endregion
    }
}
