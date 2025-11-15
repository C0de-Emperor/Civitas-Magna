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
        UnityEngine.Events.UnityAction<Scene, LoadSceneMode> callback = null;

        callback = (scene, mode) =>
        {
            SaveManager.instance.TriggerSaveLoaded(null);
            SceneManager.sceneLoaded -= callback;
        };

        SceneManager.sceneLoaded += callback;
        SceneManager.LoadScene("Game");
    }

    private void LoadGameButtonPressed()
    {
        UnityEngine.Events.UnityAction<Scene, LoadSceneMode> callback = null;

        callback = (scene, mode) =>
        {
            SaveManager.instance.TriggerSaveLoaded(SaveManager.instance.LoadData());
            SceneManager.sceneLoaded -= callback;
        };

        SceneManager.sceneLoaded += callback;
        SceneManager.LoadScene("Game");
    }
}
