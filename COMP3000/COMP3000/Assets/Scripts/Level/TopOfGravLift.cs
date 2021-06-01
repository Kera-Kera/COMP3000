using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopOfGravLift : MonoBehaviour
{
    [SerializeField]
    BoxCollider BoxCollider1 = null;
    private void OnTriggerStay(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            BoxCollider1.enabled = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            BoxCollider1.enabled = true;
        }
    }
}
