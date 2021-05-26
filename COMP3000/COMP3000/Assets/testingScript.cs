using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testingScript : MonoBehaviour
{
    private GameObject player;
    private CharacterScript playerSc;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerSc = player.GetComponent<CharacterScript>();
    }
    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position;
        transform.position += playerSc.getMovement();
    }
}
