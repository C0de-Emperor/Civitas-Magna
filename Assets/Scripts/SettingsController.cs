using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SettingsController : MonoBehaviour
{
    public Slider volumeSlider;
    public Toggle fullScreenToggle;
    public Dropdown resolutionDropdown;

    private Resolution[] resolutions;

    private void Awake()
    {
        volumeSlider.minValue = -80f;
        volumeSlider.maxValue = 0f;
        volumeSlider.onValueChanged.AddListener(SetVolume);
        fullScreenToggle.onValueChanged.AddListener(SetFullScreen);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);

        volumeSlider.value = SoundManager.instance.baseVolume;

        resolutions = Screen.resolutions.Select(resolution => new Resolution { width = resolution.width, height = resolution.height }).Distinct().ToArray(); 
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void SetVolume(float volume)
    {
        SoundManager.instance.mainMixer.SetFloat("Volume", volume);
    }

    private void SetFullScreen (bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    private void SetResolution (int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
