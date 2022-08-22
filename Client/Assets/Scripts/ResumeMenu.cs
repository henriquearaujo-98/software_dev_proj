using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void Pause(){
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void LoadSettings(){
        Debug.Log("loading settings");
    }

    public void QuitGame(){
        Debug.Log("quiting game");
        Application.Quit();
    }

    public void  diconnect(){
        Debug.Log("disconnecting");
    }
    
}
