using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{

    public static LevelManager instance;

    public GameObject loadingScreen;
    public Slider slider;

    public string[] maps;

    // Start is called before the first frame update
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        loadingScreen.SetActive(false);

    }

    public void SetServerConnectionProgress(float _progress)
    {
        slider.value = _progress;
    }

    public async Task LoadSceneAsync(int _mapID)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(maps[_mapID]);
        loadingScreen.SetActive(true);
        while (!operation.isDone)
        {
            
            float progress = (Mathf.Clamp01(operation.progress / 0.9f) / 2) + 0.5f; // 50% of the loading slide
            slider.value += progress;
            Debug.Log(progress);
            await Task.Yield();
        }

        loadingScreen.SetActive(false);
    }
}
