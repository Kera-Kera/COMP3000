using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunInfo : MonoBehaviour
{
    [SerializeField]
    private float damage;
    [SerializeField]
    private float spread;
    [SerializeField]
    private float velocity;
    [SerializeField]
    private float fireRate;
    [SerializeField]
    private float sizeOfBullet;
    [SerializeField]
    private float aiCorrection;
    [SerializeField]
    private float aiCorrectionHeight;
    [SerializeField]
    private float numberOfBullets = 1;


    public float GetDamage()
    {
        return damage;
    }

    public float GetSpread()
    {
        return spread;
    }

    public float GetVelocity()
    {
        return velocity;
    }

    public float GetFireRate()
    {
        return fireRate;
    }
    public float GetSize()
    {
        return sizeOfBullet;
    }
    public float AIDrop()
    {
        return aiCorrectionHeight;
    }
    public float AICorrection()
    {
        return aiCorrection;
    }
    public float GetNumberOfBullets()
    {
        return numberOfBullets;
    }

    private void Update()
    {
        if(transform.tag == "PickUpable" && transform.GetComponent<Rigidbody>().useGravity == true)
        {
            StartCoroutine(DespawnWeapon());
        }
    }

    IEnumerator DespawnWeapon()
    {
        yield return new WaitForSeconds(20);
        if (transform.GetComponent<Rigidbody>().useGravity == true)
        {
            Destroy(transform.gameObject);
        }
    }
}
