using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuScript : MonoBehaviour
{

    [SerializeField]
    private GameObject info;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }
    void Update()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ShowInfo()
    {
        info.SetActive(true);
    }
    public void hideInfo()
    {
        info.SetActive(false);
    }
}
