using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(SettingsController))]
public class PauseMenuManager : MonoBehaviour
{
    public Transform menuPanel;

    [HideInInspector] public bool isMenuOpen = false;

    public Button saveButton;
    public Button mainmenuButton;
    public Button quitButton;

    private SettingsController controller;

    public static PauseMenuManager instance;
    private void Awake()
    {
        controller = GetComponent<SettingsController>();

        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de PauseMenuManager dans la scène");
            return;
        }
        instance = this;

        saveButton.onClick.AddListener(() => SaveManager.instance.SaveData());
        mainmenuButton.onClick.AddListener(() => LoadMainMenu());
        quitButton.onClick.AddListener(() => Application.Quit());

        CloseMenu();
    }

    private void Update()
    {
        if (!SaveManager.instance.canSave || SaveManager.instance.isWorking)
        {
            saveButton.interactable = false;
        }
        else
        {
            saveButton.interactable = true;
        }
    }

    private void LoadMainMenu()
    {
        SaveManager.instance.canSave = false;
        SaveManager.instance.hasLoaded = false;

        SceneManager.LoadScene("MainMenu");
    }

    public void OpenMenu()
    {
        controller.UpdateUI();
        menuPanel.gameObject.SetActive(true);
        CameraController.instance.canMove = false;
        isMenuOpen = true;
    }

    public void CloseMenu()
    {
        menuPanel.gameObject.SetActive(false);
        CameraController.instance.canMove = true;
        isMenuOpen = false;
    }
}
