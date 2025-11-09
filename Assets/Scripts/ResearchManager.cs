using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResearchManager : MonoBehaviour
{
    [Header("Production")]
    public Research currentResearch;
    public float currentResearchProgress = 0f;
    public List<Research> researched = new List<Research>();
    [HideInInspector] public bool isMenuOpen = false;

    [Header("UI")]
    [SerializeField] private Transform sciencePanel;
    [SerializeField] private Button scienceButton;
    [SerializeField] private RectTransform content;
    [SerializeField] private Transform lineParent;
    [SerializeField] private Image linePrefab;
    [SerializeField] private ResearchNodeUI nodePrefab;

    [Header("Layout Settings")]
    [SerializeField] private float xSpacing = 20f;
    [SerializeField] private float ySpacing = 20f;
    private float nodeWidth;
    private float nodeHeight;

    public List<Research> allResearches;

    private Dictionary<Research, ResearchNodeUI> nodes = new();

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

        nodeWidth = (nodePrefab.transform as RectTransform).sizeDelta.x;
        nodeHeight = (nodePrefab.transform as RectTransform).sizeDelta.y;

        isMenuOpen = false;
    }

    private void Start()
    {
        GenerateTree(allResearches);
    }

    public void GenerateTree(List<Research> allResearches)
    {
        // Vide le contenu (sauf les lignes)
        foreach (Transform child in content)
        {
            if (child != lineParent)
                Destroy(child.gameObject);
        }

        nodes.Clear();

        // --- Étape 1 : Générer les nodes ---
        foreach (var research in allResearches)
        {
            var node = Instantiate(nodePrefab, content);
            node.Init(research);

            Vector2 pos = GetPosition(research);
            node.RectTransform.anchoredPosition = pos;

            nodes.Add(research, node);
        }

        // --- Étape 2 : Relier les dépendances ---
        foreach (var research in allResearches)
        {
            foreach (var dep in research.dependencies)
            {
                if (nodes.ContainsKey(dep.research) && nodes.ContainsKey(research))
                {
                    DrawDependencyLine(nodes[dep.research].RectTransform, nodes[research].RectTransform, dep.dependencyLineDepth);
                }
            }
        }
    }

    private Vector2 GetPosition(Research r)
    {
        float x = r.depth * (nodeWidth + xSpacing) - (content.sizeDelta.x/2) + nodeWidth/2 + 100f;
        float y = r.index * (nodeHeight + ySpacing) - (content.sizeDelta.y/2) + nodeHeight/2 + 100f;

        return new Vector2(x, y);
    }

    private void DrawDependencyLine(RectTransform from, RectTransform to, float depLineDepth)
    {
        Vector2 start = from.anchoredPosition;
        Vector2 end = to.anchoredPosition;

        if(depLineDepth > 0)
        {
            float x = depLineDepth * (nodeWidth + xSpacing) - (content.sizeDelta.x / 2) + nodeWidth / 2 + 100f;
            float offset = 10f;

            CreateUILine(start, new Vector2(x - offset, start.y));
            CreateUILine(new Vector2(x - offset, start.y), new Vector2(x + offset, end.y));
            CreateUILine(new Vector2(x + offset, end.y), end);
        }
        else
        {
            CreateUILine(start, end);
        }
    }

    private void CreateUILine(Vector2 start, Vector2 end)
    {
        Image line = Instantiate(linePrefab, lineParent);
        line.color = Color.white;

        Vector2 dir = end - start;
        float length = dir.magnitude;

        RectTransform rt = line.rectTransform;
        rt.sizeDelta = new Vector2(length, 3f);
        rt.anchoredPosition = start + dir / 2;
        rt.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    public void OpenResearchMenu()
    {
        isMenuOpen = true;
        sciencePanel.gameObject.SetActive(true);
        scienceButton.interactable = false;

        RefreshUI();

        SelectionManager.instance.canInteract = false;
        CameraController.instance.canMove = false;
    }

    public void StartResearch(Research research)
    {
        currentResearch = research;
        currentResearchProgress = 0f;
    }

    internal void RefreshUI()
    {
        foreach(ResearchNodeUI node in nodes.Values)
        {
            node.UpdateState();
        }
    }

    internal void ResearchComplete()
    {
        researched.Add(currentResearch);

        currentResearch = null;
        currentResearchProgress = 0f;
        RefreshUI();
    }

    internal void CloseMenu()
    {
        isMenuOpen = false;
        sciencePanel.gameObject.SetActive(false);
        scienceButton.interactable = true;

        SelectionManager.instance.canInteract = true;
        CameraController.instance.canMove = true;
    }
}


