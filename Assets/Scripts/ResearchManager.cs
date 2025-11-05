using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResearchManager : MonoBehaviour
{
    [Header("Production")]
    public Research currentResearch;
    public float currentResearchProgress = 0f;
    public List<Research> researched = new List<Research>();

    [Header("UI")]
    [SerializeField] private Transform sciencePanel;
    [SerializeField] private Button scienceButton;

    public static ResearchManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de ResearchManager dans la scène");
            return;
        }
        instance = this;

        sciencePanel.gameObject.SetActive(false);
        scienceButton.interactable = true;
        scienceButton.onClick.AddListener(() => OpenResearchMenu());
    }

    public void OpenResearchMenu()
    {
        sciencePanel.gameObject.SetActive(true);
        scienceButton.interactable = false;

        SelectionManager.instance.canInteract = false;
        CameraController.instance.canMove = false;
    }




}
