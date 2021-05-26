using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunInfo : MonoBehaviour
{
    [SerializeField]
    private float damage;
    [SerializeField]
    private float recoil;
    [SerializeField]
    private float velocity;
    [SerializeField]
    private float fireRate;
    [SerializeField]
    private float sizeOfBullet;
    [SerializeField]
    private float aiCorrection;
    [SerializeField]
    private float numberOfBullets = 1;

    public float GetDamage()
    {
        return damage;
    }

    public float GetRecoil()
    {
        return recoil;
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
        return aiCorrection;
    }
    public float GetNumberOfBullets()
    {
        return numberOfBullets;
    }
}
