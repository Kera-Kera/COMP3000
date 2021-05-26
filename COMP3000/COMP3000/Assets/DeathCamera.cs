using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCamera : MonoBehaviour
{

    // Update is called once per frame
    public void UseDeathCam()
    {
        gameObject.GetComponent<Camera>().targetDisplay = 0;
    }
    public void DisableDeathCam()
    {
        gameObject.GetComponent<Camera>().targetDisplay = 2;
    }
}
