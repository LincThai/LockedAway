//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace QuickScripts
{

    [AddComponentMenu("Quick Scripts/Quick Rotate")]
    public class QuickRotate : MonoBehaviour
    {

        public enum rotateEnum
        {
            linear,
            smoothed,
            exponential
        }
        public rotateEnum rotationType;

        public bool rotating = true;
        [Space()]
        public float rotationSpeedX;
        public bool varyingXSpeed;
        public AnimationCurve speedXRate;
        float speedXmultiplier;
        [Space()]
        public float rotationSpeedY;
        public bool varyingYSpeed;
        public AnimationCurve speedYRate;
        float speedYmultiplier;

        [Space()]

        public float rotationSpeedZ;
        public bool varyingZSpeed;
        public AnimationCurve speedZRate;
        float speedZmultiplier;



        [Header("Acceleration is used for Smoothed or Exponential Rotation")]
        public float acceleration;
        private float t;

        // For smoothed rotation
        float currentSpeedX;
        float currentSpeedY;
        float currentSpeedZ;

        private void Start()
        {
            if (!varyingXSpeed)
                speedXmultiplier = 1;

            if (!varyingYSpeed)
                speedYmultiplier = 1;

            if (!varyingZSpeed)
                speedZmultiplier = 1;

        }

        void FixedUpdate()
        {

            switch (rotationType)
            {
                case rotateEnum.linear:
                    LinearRotation();
                    break;
                case rotateEnum.smoothed:
                    SmoothedRotation();
                    break;
                case rotateEnum.exponential:
                    ExponentialRotation();
                    break;
            }
        }

        void LinearRotation()
        {
            if (rotating)
            {
                if (varyingXSpeed)
                speedXmultiplier = speedXRate.Evaluate(Time.time);

                if (varyingYSpeed)
                speedYmultiplier = speedYRate.Evaluate(Time.time);

                if (varyingZSpeed)
                speedZmultiplier = speedZRate.Evaluate(Time.time);

                transform.Rotate(rotationSpeedX * speedXmultiplier, 
                    rotationSpeedY * speedYmultiplier, 
                    rotationSpeedZ * speedZmultiplier);
            }
        }


        void SmoothedRotation()
        {
            GetT();
            currentSpeedX = Mathf.Lerp(0, rotationSpeedX, t);
            currentSpeedY = Mathf.Lerp(0, rotationSpeedY, t);
            currentSpeedZ = Mathf.Lerp(0, rotationSpeedZ, t);

            transform.Rotate(currentSpeedX, currentSpeedY, currentSpeedZ);
        }

        void GetT()
        {
            if (rotating)
            {
                t += Time.fixedDeltaTime / acceleration;
            }
            else if (!rotating)
            {
                t -= Time.fixedDeltaTime / acceleration;
            }
            else
            {
                t = 0;
            }
            t = Mathf.Clamp01(t);
        }

        void ExponentialRotation()
        {
            if (rotating)
            {
                float a = acceleration / 1000;
                rotationSpeedX += rotationSpeedX * a;
                rotationSpeedY += rotationSpeedY * a;
                rotationSpeedZ += rotationSpeedZ * a;

                transform.Rotate(rotationSpeedX, rotationSpeedY, rotationSpeedZ);
            }
        }

        #region Public Events

        public void SetRotating(bool b)
        {
            rotating = b;
        }
        public void SetRotateType(int t)
        {
            if (t < 3)
            {
                rotateEnum e = (rotateEnum)t;
                rotationType = e;
            }
            else
                Debug.LogError("You are trying to manually change Rotation Type on " + this.name + ". Integer value must be 0 - 2.");
        }
        public void SetSpeedX(float x)
        {
            rotationSpeedX = x;
        }
        public void SetSpeedY(float y)
        {
            rotationSpeedY = y;
        }
        public void SetSpeedZ(float z)
        {
            rotationSpeedZ = z;
        }
        public void SetAcceleration(float t)
        {
            acceleration = t;
        }

        #endregion
    }
}
