using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deathzone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            other.transform.GetComponent<CharacterScript>().setVelocity(new Vector3(other.transform.GetComponent<CharacterScript>().getVelocity().x, 0, other.transform.GetComponent<CharacterScript>().getVelocity().z));
            StartCoroutine(other.transform.GetComponent<CharacterScript>().Respawn());
        }
        else
        {
            Destroy(other.gameObject);
        }
    }

    
}
