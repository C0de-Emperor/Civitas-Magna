using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ResearchManager : MonoBehaviour
{
    [Header("Research")]
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

    [HideInInspector] public Research[] allResearches;

    private Dictionary<Research, ResearchNodeUI> nodes = new();

    public static ResearchManager instance;
    private void Awake()
    {
        allResearches = Resources.LoadAll<Research>("Researches");

        SaveManager.instance.OnSaveLoaded += OnLoad;

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

    private void OnLoad(SaveData data)
    {
        if(data == null)
            throw new Exception("SaveData is null");

        currentResearch = data.currentResearch;
        currentResearchProgress = data.currentResearchProgress;
        researched = data.researched.ToList();
    }

    public void GenerateTree(Research[] allResearches)
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
        foreach (Research research in allResearches)
        {
            foreach (Dependency dep in research.dependencies)
            {
                if (nodes.ContainsKey(dep.research) && nodes.ContainsKey(research))
                {
                    DrawDependencyLine(nodes[dep.research].RectTransform, nodes[research], dep.dependencyLineDepth);
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

    private void DrawDependencyLine(RectTransform from, ResearchNodeUI to, float depLineDepth)
    {
        Vector2 start = from.anchoredPosition;
        Vector2 end = to.RectTransform.anchoredPosition;
        Color color = Color.red;

        if (depLineDepth > 0)
        {
            float x = depLineDepth * (nodeWidth + xSpacing) - (content.sizeDelta.x / 2) + nodeWidth / 2 + 100f;
            float offset = 10f;

            CreateUILine(start, new Vector2(x - offset, start.y), to);
            CreateUILine(new Vector2(x - offset, start.y), new Vector2(x + offset, end.y), to);
            CreateUILine(new Vector2(x + offset, end.y), end, to);
        }
        else
        {
            CreateUILine(start, end, to);
        }
    }

    private void CreateUILine(Vector2 start, Vector2 end, ResearchNodeUI parent)
    {
        Image line = Instantiate(linePrefab, lineParent);
        line.color = new Color(0.5f, 0.45f, 0.4f, 1f);

        parent.images.Add(line);

        Vector2 dir = end - start;
        float length = dir.magnitude;

        RectTransform rt = line.rectTransform;
        rt.sizeDelta = new Vector2(length, 5f);
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


