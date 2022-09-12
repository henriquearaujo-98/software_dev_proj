using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


public class OptionsMenu : MonoBehaviour
{
   [SerializeField] Slider volumeSlider;

   public void SetVolume(float volume)
   {
       AudioListener.volume = volumeSlider.value;
   }

   public void SetQuality(int qualityIndex)
   {
       QualitySettings.SetQualityLevel(qualityIndex);
   }

   public void SetFullscreen(bool isFullscreen)
   {
       Screen.fullScreen = isFullscreen;
   }

    List<int> widths = new List<int>() {568, 960, 1280, 1920};
    List<int> heights = new List<int>() {320, 540, 800, 1080};

    public void SetScreenSize (int index) {
        {
            bool fullscreen = Screen.fullScreen;
            int width = widths[index];
            int height = heights[index];
            Screen.SetResolution(width, height, fullscreen);
        }
    }
  
}
