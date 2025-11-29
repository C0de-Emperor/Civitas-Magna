using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(SettingsController))]
public class MainMenuManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button newGameMenuButton;
    public Button startNewGameButton;
    public Button loadGameButton;
    public Button settingsButton;
    public Button backSettingsButton;
    public Button backStartNewGameButton;
    public Button quitButton;

    [Header("Panels")]
    public Transform mainMenuPanel;
    public Transform newGamePanel;
    public Transform settingsPanel;

    public Image backgroundImage;
    [HideInInspector] public Sprite[] backgroundSprites;
    [HideInInspector] public int backgroundIndex;

    [Header("Livery")]
    public Image unitBackground;
    public Image unitBorder;
    public Image unitSprite;
    public List<Livery> liveries = new List<Livery>();
    [HideInInspector] public int liveriesIndex = -1;

    [Header("Grid Size")]
    public Dropdown gridSizeDropDown;
    private GridSize selectedGridSize;
    public List<GridSize> gridSizes = new List<GridSize>();

    [Header("Fade")]
    public Animator animator;
    public Transform fadePanel;

    private SettingsController controller;

    private void Awake()
    {
        StopAllCoroutines();

        controller = GetComponent<SettingsController>();
        
        startNewGameButton.onClick.AddListener(() => StartCoroutine(StartNewGameButton()));
        newGameMenuButton.onClick.AddListener(() => OpenNewGameMenuButton());
        settingsButton.onClick.AddListener(() => OpenSettingsMenu());

        backSettingsButton.onClick.AddListener(() => BackToMainMenu());
        backStartNewGameButton.onClick.AddListener(() => BackToMainMenu());

        loadGameButton.onClick.AddListener(() => StartCoroutine(LoadGameButtonPressed()));
        quitButton.onClick.AddListener(() => Application.Quit());

        fadePanel.gameObject.SetActive(false);
        mainMenuPanel.gameObject.SetActive(true);
        settingsPanel.gameObject.SetActive(false);
        newGamePanel.gameObject.SetActive(false);

        backgroundSprites = Resources.LoadAll<Sprite>("Backgrounds");
        NextBackground();

        backgroundIndex = UnityEngine.Random.Range(0, backgroundSprites.Length-1);
        NextLivery();

        gridSizeDropDown.ClearOptions();

        List<string> options = new List<string>();

        foreach (GridSize size in gridSizes)
        {
            options.Add(size.description + " : " + size.width + " x " + size.height);
        }

        gridSizeDropDown.AddOptions(options);
        gridSizeDropDown.value = 0;
        selectedGridSize = gridSizes[0];
        gridSizeDropDown.RefreshShownValue();

        gridSizeDropDown.onValueChanged.AddListener(OnGridSizeChanged);
    }

    private void OnGridSizeChanged(int index)
    {
        GridSize selected = gridSizes[index];

        selectedGridSize = selected;
    }

    public void NextLivery()
    {
        liveriesIndex++;
        liveriesIndex %= liveries.Count;

        unitBackground.color = liveries[liveriesIndex].backgroundColor;
        unitBorder.color = liveries[liveriesIndex].spriteColor;
        unitSprite.color = liveries[liveriesIndex].spriteColor;
    }

    public void PreviousLivery()
    {
        liveriesIndex = (liveriesIndex - 1 + liveries.Count) % liveries.Count;

        unitBackground.color = liveries[liveriesIndex].backgroundColor;
        unitBorder.color = liveries[liveriesIndex].spriteColor;
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
        newGamePanel.gameObject.SetActive(false);
    }

    private void OpenNewGameMenuButton()
    {
        mainMenuPanel.gameObject.SetActive(false);
        settingsPanel.gameObject.SetActive(false);
        newGamePanel.gameObject.SetActive(true);
    }

    private void BackToMainMenu()
    {
        mainMenuPanel.gameObject.SetActive(true);
        settingsPanel.gameObject.SetActive(false);
        newGamePanel.gameObject.SetActive(false);
    }

    private IEnumerator StartNewGameButton()
    {
        fadePanel.gameObject.SetActive(true);
        animator.SetTrigger("FadeIn");

        yield return new WaitForSeconds(1f);

        UnityEngine.Events.UnityAction<Scene, LoadSceneMode> callback = null;

        SaveManager.instance.ClearAllSaveLoadedSubscribers();

        callback = (scene, mode) =>
        {
            SceneManager.sceneLoaded -= callback;
            SaveManager.instance.TriggerNewGameStarted(
                new NewGameData {
                    player = new Player ("Player", liveries[liveriesIndex] ),
                    gridSize = selectedGridSize,
                    AI_Player = new Player("AI", liveries[(liveriesIndex + 1) % liveries.Count])
                }
            );
        };

        SceneManager.sceneLoaded += callback;
        SceneManager.LoadScene("Game");
    }

    private IEnumerator LoadGameButtonPressed()
    {
        fadePanel.gameObject.SetActive(true);
        animator.SetTrigger("FadeIn");

        yield return new WaitForSeconds(1f);

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

[Serializable]
public struct GridSize
{
    public string description;
    public int width;
    public int height;
}