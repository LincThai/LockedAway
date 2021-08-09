//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{

    [AddComponentMenu("Quick Scripts/Quick Hover")]
    public class QuickHover : MonoBehaviour
    {

        public float hoverX;
        public float hoverY;
        public float hoverZ;
        public float speed = 1;
        [Range(0, 3)]
        public float offset;
        Vector3 startPos;
        private float internalTime;
        public bool isHovering = true;

        void Start()
        {
            startPos = transform.localPosition;
        }

        void FixedUpdate()
        {
            if (!isHovering)
                return;

            internalTime += Time.fixedDeltaTime * Time.timeScale;
            float x = (Mathf.Sin(offset + internalTime * speed));
            transform.localPosition = (startPos + new Vector3(hoverX * x, hoverY * x, hoverZ * x));
        }

        #region Public Events

        public void SetHovering(bool active)
        {
            isHovering = active;
        }

        public void SetHoverX(float x)
        {
            hoverX = x;
        }
        public void SetHoverY(float y)
        {
            hoverY = y;
        }
        public void SetHoverZ(float z)
        {
            hoverZ = z;
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
