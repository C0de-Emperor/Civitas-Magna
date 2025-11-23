using System;
using System.Collections.Generic;
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

    public Image backgroundImage;
    [HideInInspector] public Sprite[] backgroundSprites;
    [HideInInspector] public int backgroundIndex;

    public Image unitBackground;
    public Image unitSprite;
    public List<Livery> liveries = new List<Livery>();
    [HideInInspector] public int liveriesIndex = -1;

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

        backgroundSprites = Resources.LoadAll<Sprite>("Backgrounds");
        NextBackground();

        backgroundIndex = UnityEngine.Random.Range(0, backgroundSprites.Length-1);
        NextLivery();
    }

    public void NextLivery()
    {
        liveriesIndex++;
        liveriesIndex %= liveries.Count;

        unitBackground.color = liveries[liveriesIndex].backgroundColor;
        unitSprite.color = liveries[liveriesIndex].spriteColor;
    }

    public void PreviousLivery()
    {
        liveriesIndex--;
        liveriesIndex %= liveries.Count;

        unitBackground.color = liveries[liveriesIndex].backgroundColor;
        unitSprite.color = liveries[liveriesIndex].spriteColor;
    }

    public void NextBackground()
    {
        backgroundImage.sprite = backgroundSprites[backgroundIndex % backgroundSprites.Length];
        backgroundIndex++;
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
            SaveManager.instance.TriggerNewGameStarted(
                new NewGameData {
                    player = new Player ("eeee", liveries[liveriesIndex] )
                }
            );
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
