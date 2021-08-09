//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickScripts{

    [AddComponentMenu("Quick Scripts/Quick SetParent")]
    [RequireComponent (typeof(Collider))]
    [RequireComponent (typeof (QuickGizmo))]
    public class QuickSetParent : MonoBehaviour {

	public Transform parentTransform;
	public List<QSParent_TagToTransform> tagsAndTransforms = new List<QSParent_TagToTransform>();

	private List<QSParent_ObjectAndParent> objectsAndParents = new List<QSParent_ObjectAndParent>();

	List<string> registeredTags = new List<string>();

        //List<Collider> capturedColliders = new List<Rigidbody>();
       // Vector3 frameVelocity;
       // Vector3 prevFramePos;

    // Use this for initialization
        void Start () {
		if (parentTransform.localScale != Vector3.one)
			Debug.LogWarning ("QuickSetParent | The scale transform of " + this.name + " is not a value of 1,1,1. " +
				"This can have unwanted effects on the scaling of objects being attached to it via QuickSetParent." +
				" It is recommended to set the scale to 1,1,1.", parentTransform.gameObject);

		foreach (QSParent_TagToTransform op in tagsAndTransforms)
			registeredTags.Add (op.colliderTag);
	}

        private void FixedUpdate()
        {
          //  frameVelocity = (this.transform.position - prevFramePos);// / Time.fixedDeltaTime;

            //foreach (QSParent_ObjectAndParent obp in objectsAndParents)
            //{
            //    obp.targetObject.position = obp.targetObject.position + frameVelocity;
            //    obp.targetObject.rotation = this.transform.rotation;
            //}
           
        }

        private void LateUpdate()
        {
         //   prevFramePos = this.transform.position;

        }

        void OnTriggerEnter(Collider other){
		if (registeredTags.Contains (other.tag)) {
			Transform currentTransform = other.transform;
			Transform masterTransform = currentTransform; // The 'overall transform' for the collider we detected


			foreach (QSParent_TagToTransform op in tagsAndTransforms) {
				if (other.tag == op.colliderTag) {

					for (int i = 0; i < op.hierarchyStepsUp; i++)
						masterTransform = currentTransform.parent;

				}
			}

			AddToRegistry (masterTransform, masterTransform.parent, other);

                // THE BIG BIT - Actually setting the parent
			masterTransform.SetParent (parentTransform);

               // if (masterTransform.GetComponent<CharacterController>())
                 //   masterTransform.GetComponent<CharacterController>().

        }

	}

	void AddToRegistry(Transform target, Transform originalParent, Collider col)
	{
		QSParent_ObjectAndParent newOP = new QSParent_ObjectAndParent ();
		newOP.targetObject = target; // this is the masterTransform
		newOP.originalParent = originalParent;
		newOP.originalSiblingIndex = target.GetSiblingIndex ();
		newOP.taggedCollider = col;

          //  if (target.GetComponent<Rigidbody>())
          //      capturedRBs.Add(target.GetComponent<Rigidbody>());

		objectsAndParents.Add (newOP);
	}

	void OnTriggerExit (Collider other)
	{
		if (registeredTags.Contains (other.tag)) {
			foreach (QSParent_ObjectAndParent obp in objectsAndParents) {
				if (other == obp.taggedCollider) {
					ReleaseFromQuickParent (obp);

                       // Rigidbody check = other.transform.GetComponent<Rigidbody>();

                       // if (check != null && capturedRBs.Contains(check))
                       //     capturedRBs.Remove(check);

					objectsAndParents.Remove (obp);
					break;
				}
			}
		}

		objectsAndParents.Sort ();
       //     capturedRBs.Sort();
	}

	void ReleaseFromQuickParent (QSParent_ObjectAndParent targetObp)
	{
		targetObp.targetObject.SetParent (targetObp.originalParent);
		targetObp.targetObject.SetSiblingIndex (targetObp.originalSiblingIndex);

	}

		public void ExternalAddTag(string tag)
		{
			QSParent_TagToTransform ttt = new QSParent_TagToTransform();
			ttt.colliderTag = tag;
			ttt.hierarchyStepsUp = 0;

			tagsAndTransforms.Add(ttt);
		}

		public void ExternalRemoveTag(string tag)
		{
			if (tag == null || tagsAndTransforms.Count == 0)
				return;

			foreach (QSParent_TagToTransform ttt in tagsAndTransforms)
				if (ttt.colliderTag == tag) {
					tagsAndTransforms.Remove (ttt);
					break;
				}

			//tagsAndTransforms.Sort ();
		}

		public void ExternalRemoveLast()
		{
			if (tagsAndTransforms.Count > 0)
							tagsAndTransforms.RemoveAt (tagsAndTransforms.Count - 1);
		}
}
}
