using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bean"))
        {
            BeanController.DestroyBean(other.transform.GetComponent<BeanObject>());
        }
    }
}
