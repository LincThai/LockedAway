//Quick Scripts v2 by Jack Wilson, Navlight Games 2021.
//Thank you for purchasing this product.

using System.Collections;
using UnityEngine;

namespace QuickScripts{

[System.Serializable]
	public class QSParent_TagToTransform {
		
	public string colliderTag;
	public int hierarchyStepsUp;

	}

[System.Serializable]
public class QSParent_ObjectAndParent {
	public Collider taggedCollider;
	public Transform targetObject;
	public Transform originalParent;
	public int originalSiblingIndex;
	}
}
