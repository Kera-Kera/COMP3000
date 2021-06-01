using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravLift : MonoBehaviour
{

    Vector3 velocity;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            velocity = other.gameObject.GetComponent<CharacterScript>().getVelocity();
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            if (velocity.y < -5)
            {
                velocity.y = -4;
            }
            velocity.y += 7 * Time.deltaTime;
            other.gameObject.GetComponent<CharacterController>().Move(velocity * Time.deltaTime);
        }
    }

}
