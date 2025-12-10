using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/ProductionItem/Building")]
public class BuildingProductionItem : CityProductionItem
{
    public int ID = -1;
    [Header("Cost")]
    public float costInGoldPerTurn;
    [Header("Bonuses")]
    public float bonusFood;
    public float bonusProduction;
    public float bonusGold;
    public float bonusScience;
    public int bonusHealth;
    [Header("Requirements")]
    public List<BuildingProductionItem> buildingRequierments = new List<BuildingProductionItem>();

    public override void OnProductionComplete(City city)
    {
        city.builtBuildings.Add(this);
        city.UpdateBanner();
        Debug.Log($"{city.cityName} a construit {itemName}");
    }
}
