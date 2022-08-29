using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResumeMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();

            }else{

                Pause();

            }
        }
    }

    public void Resume(){
        pauseMenuUI.SetActive(false);
        GameIsPaused = false;
    }

    public void Pause(){
        pauseMenuUI.SetActive(true);
        GameIsPaused = true;
    }

    public void QuitGame(){
        Debug.Log("quiting game");
        Application.Quit();
    }

    public void diconnect(){
        Debug.Log("disconnecting");
        Client.instance.Disconnect();
        Destroy(GameObject.FindGameObjectWithTag("Player"), 0f);
        SceneManager.LoadScene("MainMenu");
    }
}
