using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{
    public class QS_DemoBall : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(DestroySelf());
        }

        IEnumerator DestroySelf()
        {
            yield return new WaitForSecondsRealtime(10);
            Destroy(this.gameObject);
        }
    }
}
