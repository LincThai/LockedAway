//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{

    [AddComponentMenu("Quick Scripts/Quick LookAt")]
    public class QuickLookAt : MonoBehaviour
    {
        public Transform target;

        public bool lockX;
        public bool lockY;
        public bool lockZ;
        public bool lookOppositeDirection;

        [Header("0 = Snap to face object | 1 = Do not turn at all")]
        [Range(0, 1)]
        public float rotationDamping;

        Quaternion targetRot;


        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (lookOppositeDirection)
                targetRot = Quaternion.LookRotation(transform.position - target.position);
            else
                targetRot = Quaternion.LookRotation(target.position - transform.position);

            if (lockX) targetRot.x = 0;
            if (lockY) targetRot.y = 0;
            if (lockZ) targetRot.z = 0;

            //transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * (1 - rotationDamping));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, (Time.deltaTime / rotationDamping) * (1- rotationDamping));
        }
    }

}
