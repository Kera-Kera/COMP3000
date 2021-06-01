using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] spawnableGuns;

    private Vector3[] savedLocations = new Vector3[5];
    private Quaternion[] savedRotations = new Quaternion[5];

    private bool[] isSpawning = new bool[5];

    [SerializeField]
    private Transform itemPickups;
    // Start is called before the first frame update
    void Start()
    {
        spawnableGuns = GameObject.FindGameObjectsWithTag("PickUpable");
        for (int i = 0; i < spawnableGuns.Length; i++)
        {
            savedLocations[i] = spawnableGuns[i].transform.position;
            savedRotations[i] = spawnableGuns[i].transform.rotation;
            isSpawning[i] = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < spawnableGuns.Length; i++)
        {
            if(spawnableGuns[i].tag != "PickUpable" && !isSpawning[i])
            {
                isSpawning[i] = true;
                StartCoroutine(StartSpawnTimer(i));
            }
        }
    }

    IEnumerator StartSpawnTimer(int i)
    {
        yield return new WaitForSeconds(30);
        GameObject spawnedWeapon = Instantiate(spawnableGuns[i], savedLocations[i], savedRotations[i], itemPickups);
        spawnedWeapon.tag = "PickUpable";
        spawnedWeapon.GetComponent<Rigidbody>().useGravity = false;
        spawnedWeapon.GetComponent<Rigidbody>().isKinematic = true;
        spawnedWeapon.GetComponent<BoxCollider>().enabled = true;
        spawnedWeapon.name = spawnableGuns[i].name;
        spawnableGuns[i] = spawnedWeapon;
        isSpawning[i] = false;
    }
}
