using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CityBorders))]
public class City : MonoBehaviour
{
    public string cityName = "Default";

    [Header("Production")]
    private float food = 2f;
    private float production = 1f;
    private float gold = 0f;
    private float science = 0f;
    [Header("Population")]
    public int population = 1;
    public float foodStock;
    [Header("Health")]
    [HideInInspector] public float health;
    [HideInInspector] public float maxHealth = 200f;


    [HideInInspector] 
    public Dictionary<Vector2, HexCell> controlledTiles = new Dictionary<Vector2, HexCell>();
    [HideInInspector] 
    public CityBorders borders;
    [HideInInspector]
    public CityBannerUI bannerUI;

    private void Awake()
    {
        health = maxHealth;
        TurnManager.instance.OnTurnChange += UpdateFoodStock;
        TurnManager.instance.OnTurnChange += UpdateBanner;
    }

    public float GetCityFoodProduction()
    {
        float amount = food;

        foreach (HexCell cell in controlledTiles.Values)
        {
            // tile bonus
            amount += cell.food;
            // base amount
            amount += cell.terrainType.food;
        }

        return amount;
    }

    public float GetCityProduction()
    {
        float amount = production;

        foreach (HexCell cell in controlledTiles.Values)
        {
            // tile bonus
            amount += cell.production;
            // base amount
            amount += cell.terrainType.production;
        }

        return amount;
    }

    public float GetCityGoldProduction()
    {
        return gold;
    }

    public float GetCityScienceProduction()
    {
        return science;
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
        health -= 1;

        int turns = GetTurnsToNextPopulation();

        bannerUI.UpdateInfo(cityName, population, turns, health, maxHealth);
    }
}
