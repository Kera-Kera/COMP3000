using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testingScript : MonoBehaviour
{
    private GameObject player;
    private GameObject AI;
    private CharacterScript playerSc;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        AI = GameObject.FindGameObjectWithTag("AI");
        playerSc = player.GetComponent<CharacterScript>();
    }
    // Update is called once per frame
    void Update()
    {
        transform.position = AI.transform.position;
        //transform.position += playerSc.getMovement();

        Vector3 playerDir = AI.transform.position - player.transform.position;

        transform.position += playerDir.normalized;
    }
}
