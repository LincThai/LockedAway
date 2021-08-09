//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{
    [AddComponentMenu("Quick Scripts/Quick Follow")]
    public class QuickFollow : MonoBehaviour
    {

        public Transform target;

        public bool stickToTarget = true;
        public float minimumDistance;
        [Header("If not sticking to target, how much damping should be applied?")]
        [Range(0, 1)]
        public float movementDamping;

        [Space(20)]
        public bool copyRotation;
        [Range(0, 1)]
        public float rotationDamping;


        Vector3 newPos;
        Quaternion newRot;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void LateUpdate()
        {

            if (stickToTarget)
                newPos = target.position;
            else if (Vector3.Distance(this.transform.position, target.position) < minimumDistance)
                return;
            else
                newPos = Vector3.Lerp(this.transform.position, target.position, (Time.deltaTime / movementDamping) * (1 - movementDamping));


            this.transform.position = newPos;


            if (copyRotation)
            {
                newRot = Quaternion.Slerp(this.transform.rotation, target.rotation, (Time.deltaTime / rotationDamping) * (1 - rotationDamping));
                this.transform.rotation = newRot;
            }
        }
    }

}
