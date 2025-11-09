using System;
using TreeEditor;
using UnityEngine;
using UnityEngine.UI;

public class ResearchNodeUI : MonoBehaviour
{
    [Header("Research")]
    public Research research;

    [Header("References")]
    public Image background;
    public Image disc;
    public Image image;
    public Image icon;
    public Text nameText;
    public Text turnText;
    public Button button;
    public Slider slider;

    public Sprite discSprite;
    public Sprite knob;


    public void Init(Research _research)
    {
        research = _research;

        icon.sprite = research.icon;
        nameText.text = research.researchName;

        button.onClick.AddListener(() =>
        {
            OnButtonClick();
        });
    }

    public void UpdateState()
    {
        var manager = ResearchManager.instance;

        if (manager.researched.Contains(research))
        {
            turnText.text = "";
            background.color = new Color(0.57f, 0.49f, 0.34f, 1f);
            disc.color = new Color(0.57f, 0.49f, 0.34f, 1f);
            image.color = Color.white;
            image.sprite = discSprite;

            slider.gameObject.SetActive(false);

            button.interactable = false;
        }
        else if (manager.currentResearch == research && AreDependencyResearched(research))
        {
            if (CityManager.instance.GetTotalScienceProduction() != 0)
                turnText.text = Mathf.CeilToInt((research.scienceCost - manager.currentResearchProgress) / CityManager.instance.GetTotalScienceProduction()).ToString();
            else
                turnText.text = "_";

            background.color = new Color(0.73f, 0.67f, 0.6f, 1f); 
            disc.color = new Color(0, 0.5f, 1f, 1f);
            image.color = Color.white;
            image.sprite = discSprite;

            slider.gameObject.SetActive(true);
            slider.value = manager.currentResearchProgress / manager.currentResearch.scienceCost;

            button.interactable = false;
        }
        else if (AreDependencyResearched(research))
        {
            if (CityManager.instance.GetTotalScienceProduction() != 0)
                turnText.text = Mathf.CeilToInt(research.scienceCost / CityManager.instance.GetTotalScienceProduction()).ToString();
            else
                turnText.text = "-";

            background.color = new Color(0.64f, 0.6f, 0.55f, 1f);
            disc.color = new Color(0.17f, 0.42f, 0.71f, 1f);
            image.color = Color.white;
            image.sprite = discSprite;

            slider.gameObject.SetActive(true);
            slider.value = 0f;

            button.interactable = true;
        }
        else
        {
            if (CityManager.instance.GetTotalScienceProduction() != 0)
                turnText.text = Mathf.CeilToInt(research.scienceCost / CityManager.instance.GetTotalScienceProduction()).ToString();
            else
                turnText.text = "-";

            background.color = new Color(0.36f, 0.35f, 0.34f, 1f); 
            disc.color = new Color(0.42f, 0.42f, 0.42f, 1f);
            image.color = new Color(0.42f, 0.42f, 0.42f, 1f);
            image.sprite = knob;

            slider.gameObject.SetActive(false);
            slider.value = 0f;

            button.interactable = false;
        }
    }

    private bool AreDependencyResearched(Research research)
    {
        if ( research.dependencies == null || research.dependencies.Length == 0)
            return true;

        foreach (Dependency dep in research.dependencies)
        {
            if (!ResearchManager.instance.researched.Contains(dep.research))
                return false;
        }
        return true;
    }

    public void OnButtonClick()
    {
        if (!ResearchManager.instance.researched.Contains(research) && ResearchManager.instance.currentResearch != research)
        {
            ResearchManager.instance.StartResearch(research);
            ResearchManager.instance.RefreshUI();
        }
    }

    public RectTransform RectTransform => transform as RectTransform;
}
