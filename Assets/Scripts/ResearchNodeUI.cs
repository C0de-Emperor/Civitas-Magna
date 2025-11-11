using System.Collections.Generic;
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
    public Sprite p;

    [HideInInspector] public List<Image> images = new List<Image>();

    public enum State { Researched, InResearch, ToResearch, Blocked }
    public State state;

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
        if (manager == null || research == null) return;

        // newState
        State newState;
        if (manager.researched.Contains(research))
            newState = State.Researched;
        else if (manager.currentResearch == research && AreDependencyResearched(research))
            newState = State.InResearch;
        else if (AreDependencyResearched(research))
            newState = State.ToResearch;
        else
            newState = State.Blocked;

        // MAJ "dynamiques"
        float totalScience = CityManager.instance != null ? CityManager.instance.GetTotalScienceProduction() : 0f;

        switch (newState)
        {
            case State.Researched:
                turnText.text = "";
                slider.gameObject.SetActive(false);
                break;

            case State.InResearch:
                if (totalScience > 0f)
                {
                    int turns = Mathf.CeilToInt((research.scienceCost - manager.currentResearchProgress) / totalScience);
                    turnText.text = Mathf.Max(0, turns).ToString();
                }
                else
                {
                    turnText.text = "_";
                }

                slider.gameObject.SetActive(true);
                slider.value = Mathf.Clamp01(manager.currentResearchProgress / Mathf.Max(0.0001f, manager.currentResearch.scienceCost));
                break;

            case State.ToResearch:
                if (totalScience > 0f)
                {
                    int turns = Mathf.CeilToInt(research.scienceCost / totalScience);
                    turnText.text = Mathf.Max(0, turns).ToString();
                }
                else
                {
                    turnText.text = "-";
                }

                slider.gameObject.SetActive(true);
                slider.value = 0f;
                break;

            case State.Blocked:
            default:
                if (totalScience > 0f)
                {
                    int turns = Mathf.CeilToInt(research.scienceCost / totalScience);
                    turnText.text = Mathf.Max(0, turns).ToString();
                }
                else
                {
                    turnText.text = "-";
                }

                slider.gameObject.SetActive(false);
                slider.value = 0f;
                break;
        }

        // appliquer changements "statiques"
        if (newState != state)
        {
            state = newState;

            switch (state)
            {
                case State.Researched:
                    background.color = new Color(0.57f, 0.49f, 0.34f, 1f);
                    disc.color = new Color(0.57f, 0.49f, 0.34f, 1f);
                    image.color = Color.white;
                    image.sprite = discSprite;
                    button.interactable = false;
                    foreach (Image im in images)
                    {
                        im.color = new Color(0.57f, 0.49f, 0.34f, 1f);
                        im.sprite = null;
                        im.transform.SetAsLastSibling();
                    }
                    break;

                case State.InResearch:
                    background.color = new Color(0.73f, 0.67f, 0.6f, 1f);
                    disc.color = new Color(0, 0.5f, 1f, 1f);
                    image.color = Color.white;
                    image.sprite = discSprite;
                    button.interactable = false;
                    foreach (Image im in images) 
                    { 
                        im.color = Color.white;
                        im.sprite = null;
                    }
                    break;

                case State.ToResearch:
                    background.color = new Color(0.64f, 0.6f, 0.55f, 1f);
                    disc.color = new Color(0.17f, 0.42f, 0.71f, 1f);
                    image.color = Color.white;
                    image.sprite = discSprite;
                    button.interactable = true;
                    foreach (Image im in images)
                    {
                        im.color = Color.white;
                        im.sprite = null;
                    }
                    break;

                case State.Blocked:
                default:
                    background.color = new Color(0.36f, 0.35f, 0.34f, 1f);
                    disc.color = new Color(0.42f, 0.42f, 0.42f, 1f);
                    image.color = new Color(0.42f, 0.42f, 0.42f, 1f);
                    image.sprite = knob;
                    button.interactable = false;
                    foreach (Image im in images) 
                    {
                        im.color = new Color(0.37f, 0.33f, 0.27f, 1f);
                        im.sprite = p;
                    }
                    break;
            }
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
