using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour
{

    Vector3 startingPos = new Vector3();
    Vector3 tempPos = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        startingPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Animation();
    }

    private void Animation()
    {
        transform.Rotate(new Vector3(0f, Time.deltaTime * 40f, 0f));

        tempPos = startingPos;
        tempPos.y += Mathf.Sin(Time.fixedTime * 2f) * 0.2f;

        transform.position = tempPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (transform.GetChild(0).gameObject.activeSelf)
        {
            if (other.transform.CompareTag("Player") || other.transform.CompareTag("AI"))
            {
                other.gameObject.GetComponent<BaseCharScript>().takeHeal(50f);
                StartCoroutine(WaitForSpawn());
                transform.GetChild(0).gameObject.SetActive(false);
                transform.GetChild(1).gameObject.SetActive(false);
            }
        }
    }

    public bool isRespawning()
    {
        return !transform.GetChild(0).gameObject.activeSelf;
    }

    IEnumerator WaitForSpawn()
    {
        yield return new WaitForSeconds(15);
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(true);
    }
}
