using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button startButton;

    private void Awake()
    {
        startButton.onClick.AddListener(() => StartButtonPressed());
    }

    private void StartButtonPressed()
    {
        SceneManager.LoadScene(1);
    }
}
