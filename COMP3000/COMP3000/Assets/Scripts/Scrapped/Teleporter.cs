using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField]
    private GameObject TargetTeleporter;
    [SerializeField]
    private float TeleporterCooldown = 2;
    private bool isTeleportOnCooldown = false;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isTeleportOnCooldown)
        {
            other.gameObject.transform.position = TargetTeleporter.transform.position;
            other.gameObject.transform.rotation = new Quaternion(0f, 180f, 0f, 0f);
            StartCoroutine(WaitForSpawn());
            TargetTeleporter.GetComponent<Teleporter>().PassWaitForSpawn();
        }
    }

    public void PassWaitForSpawn()
    {
        StartCoroutine(WaitForSpawn());
    }

    IEnumerator WaitForSpawn()
    {
        isTeleportOnCooldown = true;
        yield return new WaitForSeconds(TeleporterCooldown);
        isTeleportOnCooldown = false;
    }
}
