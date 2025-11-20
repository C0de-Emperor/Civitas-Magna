using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(SettingsController))]
public class MainMenuManager : MonoBehaviour
{
    public Button newGameButton;
    public Button loadGameButton;
    public Button settingsButton;
    public Button backSettingsButton;
    public Button quitButton;

    public Transform mainMenuPanel;
    public Transform settingsPanel;

    private SettingsController controller;

    private void Awake()
    {
        controller = GetComponent<SettingsController>();

        newGameButton.onClick.AddListener(() => NewGameButtonPressed());
        loadGameButton.onClick.AddListener(() => LoadGameButtonPressed());
        settingsButton.onClick.AddListener(() => OpenSettingsMenu());
        backSettingsButton.onClick.AddListener(() => CloseSettingsMenu());
        quitButton.onClick.AddListener(() => Application.Quit());

        mainMenuPanel.gameObject.SetActive(true);
        settingsPanel.gameObject.SetActive(false);
    }

    private void OpenSettingsMenu()
    {
        controller.UpdateUI();
        mainMenuPanel.gameObject.SetActive(false);
        settingsPanel.gameObject.SetActive(true);
    }

    private void CloseSettingsMenu()
    {
        mainMenuPanel.gameObject.SetActive(true);
        settingsPanel.gameObject.SetActive(false);
    }

    private void NewGameButtonPressed()
    {
        UnityEngine.Events.UnityAction<Scene, LoadSceneMode> callback = null;

        SaveManager.instance.ClearAllSaveLoadedSubscribers();

        callback = (scene, mode) =>
        {
            SceneManager.sceneLoaded -= callback;
            SaveManager.instance.TriggerSaveLoaded(null);
        };

        SceneManager.sceneLoaded += callback;
        SceneManager.LoadScene("Game");
    }

    private void LoadGameButtonPressed()
    {
        UnityEngine.Events.UnityAction<Scene, LoadSceneMode> callback = null;

        SaveManager.instance.ClearAllSaveLoadedSubscribers();

        callback = (scene, mode) =>
        {
            SceneManager.sceneLoaded -= callback;
            SaveManager.instance.TriggerSaveLoaded(SaveManager.instance.LoadData());
        };

        SceneManager.sceneLoaded += callback;
        SceneManager.LoadScene("Game");
    }
}
