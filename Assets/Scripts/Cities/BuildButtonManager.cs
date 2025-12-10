using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildButtonManager : MonoBehaviour
{
    [Header("References")]
    private ProductionButton[] buildingButtons;
    private ProductionButton[] unitButtons;
    public RectTransform buildingContentPanel;
    public RectTransform unitContentPanel;
    public Transform bonusPrefab;
    public Transform buildButtonPrefab;

    [Header("Colors")]
    public Color activeColor = new Color(1, 0.8f, 0.8f, 1);
    public Color unactiveColor = new Color(0.8f, 0.8f, 0.8f, 1);
    public Color builtColor = new Color(0.5f, 1f, 0.5f, 1);

    [Header("Sprites")]
    public Sprite selectedSprite;
    public Sprite unselectedSprite;

    public Sprite food;
    public Sprite prod;
    public Sprite gold;
    public Sprite science;
    public Sprite health;

    [Header("Data")]
    private BuildingProductionItem[] allBuildings;
    private UnitProductionItem[] allUnits;


    public static BuildButtonManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de BuildButtonManager dans la scène !");
            return;
        }
        instance = this;

        // -- Chargement des données --
        allBuildings = Resources.LoadAll<BuildingProductionItem>("Production/Buildings");
        allUnits = Resources.LoadAll<UnitProductionItem>("Production/Units");

        // -- Buildings --
        if (allBuildings == null || allBuildings.Length == 0)
        {
            Debug.LogWarning("Aucun BuildingProductionItem trouvé dans 'Resources/Buildings' !");
            return;
        }

        Array.Sort(allBuildings, (a, b) => a.ID.CompareTo(b.ID));
        buildingButtons = new ProductionButton[allBuildings.Length];

        for (int i = 0; i < allBuildings.Length; i++)
        {
            var buttonGO = Instantiate(buildButtonPrefab, buildingContentPanel);
            var button = buttonGO.GetComponent<ProductionButton>();

            if (button == null)
            {
                Debug.LogError($"Le prefab {buildButtonPrefab.name} n’a pas de ProductionButton !");
                continue;
            }

            button.item = allBuildings[i];
            button.Init(allBuildings[i]);

            buildingButtons[i] = button;
        }

        // -- Units --
        if (allUnits == null || allUnits.Length == 0)
        {
            Debug.LogWarning("Aucun UnitProductionItem trouvé dans 'Resources/Units' !");
            return;
        }

        unitButtons = new ProductionButton[allUnits.Length];

        for (int i = 0; i < allUnits.Length; i++)
        {
            var buttonGO = Instantiate(buildButtonPrefab, unitContentPanel);
            var button = buttonGO.GetComponent<ProductionButton>();

            if (button == null)
            {
                Debug.LogError($"Le prefab {buildButtonPrefab.name} n’a pas de ProductionButton !");
                continue;
            }

            button.item = allUnits[i];
            button.Init(allUnits[i]);

            unitButtons[i] = button;
        }
    }

    public void RefreshUI(bool isBuildingMenu)
    {
        ProductionButton[] buttons = isBuildingMenu ? buildingButtons : unitButtons;
        if (buttons == null || buttons.Length == 0)
            return;

        City city = CityManager.instance.openedCity;
        if (city == null)
            return;

        int showedButton = 0;
        float buttonHeight = buttons[0].GetComponent<RectTransform>().rect.height;
        float spacing = 5f; // même valeur spacing

        foreach (ProductionButton button in buttons)
        {
            bool shouldBeVisible = true;

            // --- Conditions d'affichage : bâtiments requis ---
            if (button.item is BuildingProductionItem building)
            {
                foreach (BuildingProductionItem requirement in building.buildingRequierments)
                {
                    if (!city.builtBuildings.Contains(requirement))
                    {
                        shouldBeVisible = false;
                        break;
                    }
                }
            }

            // --- Condition de recherche requise ---
            if(button.item.requiredReserch != null)
            {
                if (!ResearchManager.instance.researched.Contains(button.item.requiredReserch))
                {
                    shouldBeVisible = false;
                }
            }

            button.gameObject.SetActive(shouldBeVisible);

            if (shouldBeVisible)
            {
                showedButton++;
                button.UpdateVisual(selectedSprite, activeColor, unselectedSprite, unactiveColor, builtColor);
            }
        }

        // --- Ajustement ContentPanel---
        if (showedButton > 0)
        {
            float totalHeight = buttonHeight * showedButton + spacing * (showedButton - 1);

            RectTransform contentRect = (isBuildingMenu ? buildingContentPanel : unitContentPanel);
            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
        }
    }

    public UnitProductionItem GetSettlerProductionItem()
    {
        foreach(UnitProductionItem item in allUnits)
        {
            if (item.unit is CivilianUnitType civilianUnit)
            {
                if (civilianUnit.job == CivilianUnitType.CivilianJob.Settler)
                    return item;
            }
        }

        throw new Exception("Settler not in allUnits");
    }

    internal UnitProductionItem GetRandomMilitaryUnit()
    {
        List<UnitProductionItem> items = new List<UnitProductionItem>();

        foreach(UnitProductionItem item in allUnits)
        {
            if (item.unit is MilitaryUnitType militaryUnit)
            {
                if(militaryUnit.IsABoat == false)
                    items.Add(item);
            }
        }

        return items[UnityEngine.Random.Range(0, items.Count)];
    }
}
