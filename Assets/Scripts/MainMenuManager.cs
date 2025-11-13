using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button newGameButton;
    public Button loadGameButton;

    private void Awake()
    {
        newGameButton.onClick.AddListener(() => NewGameButtonPressed());
        loadGameButton.onClick.AddListener(() => LoadGameButtonPressed());
    }

    private void NewGameButtonPressed()
    {
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            SaveManager.instance.TriggerSaveLoaded(null);
            SceneManager.sceneLoaded -= (scene, mode) => { };
        };

        SceneManager.LoadScene(1);
    }

    private void LoadGameButtonPressed()
    {
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            SaveManager.instance.TriggerSaveLoaded(SaveManager.instance.LoadData());
            SceneManager.sceneLoaded -= (scene, mode) => { };
        };


        SceneManager.LoadScene("Game");
    }
}
