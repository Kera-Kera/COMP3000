using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMode : MonoBehaviour
{

    [SerializeField]
    private Transform player;
    [SerializeField]
    private Transform AI;

    private bool winnable = true;
    [SerializeField]
    private Transform Crosshair;
    [SerializeField]
    private Transform WhosWins;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        if (player.GetComponent<CharacterScript>().GetPlayerScore() == 10 && winnable)
        {
            winnable = false;
            WhosWins.GetComponent<Text>().text = "Player Wins!";
        }
        if (AI.GetComponent<BaseAIScript>().GetAIScore() == 10 && winnable)
        {
            winnable = false;
            WhosWins.GetComponent<Text>().text = "AI Wins!";
        }


        if(!winnable)
        {
            WhosWins.parent.gameObject.SetActive(true);
            player.gameObject.SetActive(false);
            AI.gameObject.SetActive(false);
            Crosshair.gameObject.SetActive(false);
            GetComponent<Camera>().targetDisplay = 0;
            transform.parent.Rotate(0, 20 * Time.deltaTime, 0);
            endGame();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene(0);
            }
        }
    }

    IEnumerator endGame()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(0);
    }
}
