using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CityBorders))]
public class City : MonoBehaviour
{
    const float RUINED_CITY_FACTOR_REDUCTION = 0.5f;
    const float FACTOR_REGENERATION = 0.05f;

    public string cityName = "Default";
    [SerializeField] private Transform model;
    public HexCell occupiedCell;
    public Player master;

    [Header("Base Production Point")]
    private float baseFood = 2f;
    private float baseProduction = 1f;
    private float baseGold = 0f;
    private float baseScience = 0f;

    [Header("Population")]
    public int population = 1;
    public float foodStock;

    [Header("Health")]
    public float damage = 0;
    private float maxHealth = 200f;

    [Header("Production")]
    public CityProductionItem currentProduction;
    public float currentProductionProgress = 0f;
    public List<BuildingProductionItem> builtBuildings = new List<BuildingProductionItem>();

    public float cityFactor = 1;

    [HideInInspector] public Dictionary<Vector2, HexCell> controlledTiles = new Dictionary<Vector2, HexCell>();
    [HideInInspector] public CityBorders borders;
    [HideInInspector] public CityBannerUI bannerUI;



    private void Awake()
    {
        damage = 0f;
        TurnManager.instance.OnTurnChange += UpdateFoodStock;
        TurnManager.instance.OnTurnChange += AddTurnProduction;
        TurnManager.instance.OnTurnChange += AddTurnGoldProdution;
        TurnManager.instance.OnTurnChange += AddTurnScienceProdution;
        TurnManager.instance.OnTurnChange += UpdateBanner;

        TurnManager.instance.OnTurnChange += RegenerateFactors;
    }

    public float GetCityFoodProduction()
    {
        float amount = baseFood;

        foreach (BuildingProductionItem building in builtBuildings)
        {
            amount += building.bonusFood;
        }

        foreach (HexCell cell in controlledTiles.Values)
        {
            // tile bonus
            amount += cell.food;
            // base amount
            amount += cell.terrainType.food;
        }

        return amount * cityFactor;
    }

    public float GetCityProduction()
    {
        float amount = baseProduction;

        foreach (BuildingProductionItem building in builtBuildings)
        {
            amount += building.bonusProduction;
        }

        foreach (HexCell cell in controlledTiles.Values)
        {
            // tile bonus
            amount += cell.production;
            // base amount
            amount += cell.terrainType.production;
        }

        return amount * cityFactor;
    }

    public float GetCityGoldProduction()
    {
        float amount = baseGold;

        amount += population * 0.25f;

        foreach (BuildingProductionItem building in builtBuildings)
        {
            amount += building.bonusGold;
            amount -= building.costInGoldPerTurn;
        }

        return amount * cityFactor;
    }

    public float GetCityScienceProduction()
    {
        float amount = baseScience;

        amount += population;

        foreach (BuildingProductionItem building in builtBuildings)
        {
            amount += building.bonusScience;
        }

        return amount * cityFactor;
    }

    public float GetCityMaxHealth()
    {
        float amount = maxHealth;

        foreach (BuildingProductionItem building in builtBuildings)
        {
            amount += building.bonusHealth;
        }

        return amount;
    }



    public void UpdateFoodStock()
    {
        float net = GetCityFoodProduction() - population * 2f;

        // Gain de nourriture ce tour
        foodStock += net;


        // Tant que la réserve permet d'augmenter la population, on augmente
        // Utilise >= pour éviter les off-by-one quand foodStock est exactement la valeur requise

        if(net > 0)
        {
            while (foodStock >= CityManager.instance.PopulationFunction(population + 1))
            {
                foodStock -= CityManager.instance.PopulationFunction(population);
                population++;
                borders.ExpandCity();
            }
        }
        else if (net < 0)
        {
            // Si on a trop perdu et qu'on tombe en dessous de zéro
            while (foodStock < 0 && population > 1)
            {
                population--;
                Debug.LogWarning($"{cityName} a perdu 1 population à cause de la famine !");

                // On redonne un peu de stock (nouvelle capacité réduite)
                foodStock += CityManager.instance.PopulationFunction(population);
            }

            // Clamp pour éviter les valeurs négatives folles
            if (foodStock < 0)
                foodStock = 0f;
        }
    }

    private int GetTurnsToNextPopulation()
    {
        float nextPopReq = CityManager.instance.PopulationFunction(population + 1) - foodStock;
        float net = GetCityFoodProduction() - population * 2f;

        if (net > 0f)
        {
            return Mathf.CeilToInt(nextPopReq / net);
        }

        if (Mathf.Approximately(net, 0f))
            return 0; // stagnation

        // famine
        int turnsToLosePop = Mathf.CeilToInt(foodStock / Mathf.Abs(net));
        return -Mathf.Clamp(turnsToLosePop, 1, 9999);
    }

    public void UpdateBanner()
    {
        damage += 1;

        int turns = GetTurnsToNextPopulation();
        if(currentProduction != null)
            bannerUI.UpdateInfo(
                cityName, 
                population, 
                turns, 
                damage, 
                GetCityMaxHealth(), 
                currentProduction.icon, 
                CityManager.instance.GetTurnsToProduce(currentProduction, this)
            );
        else
            bannerUI.UpdateInfo(
                cityName, 
                population, 
                turns, 
                damage, 
                GetCityMaxHealth(), 
                null, 
                -1
            );
    }

    public void SetProduction(CityProductionItem item)
    {
        currentProduction = item;
        currentProductionProgress = 0f;

        UpdateBanner();
    }



    public void HideForOverlay()
    {
        model.gameObject.SetActive(false);
    }

    public void ShowForOverlay()
    {
        model.gameObject.SetActive(true);
    }

    private void AddTurnProduction()
    {
        if(currentProduction == null)
            return;

        currentProductionProgress += GetCityProduction();

        if(currentProductionProgress >= currentProduction.costInProduction)
        {
            currentProduction.OnProductionComplete(this);
            SetProduction(null);
        }
    }

    private void AddTurnGoldProdution()
    {
        PlayerManager.instance.goldStock += GetCityGoldProduction();
    }

    private void AddTurnScienceProdution()
    {
        if(ResearchManager.instance.currentResearch != null)
            ResearchManager.instance.currentResearchProgress += GetCityScienceProduction();
    }

    public void TakeDamage(Unit unit)
    {
        damage += unit.GetUnitMilitaryData().AttackPower;

        if (damage >= maxHealth)
        {
            master = unit.master;
            damage = Mathf.Round(maxHealth/2);
            cityFactor -= RUINED_CITY_FACTOR_REDUCTION;
        }
    }

    public void RegenerateFactors()
    {
        cityFactor += FACTOR_REGENERATION;

        if (cityFactor > 1)
        {
            cityFactor = 1;
        }
    }
}
