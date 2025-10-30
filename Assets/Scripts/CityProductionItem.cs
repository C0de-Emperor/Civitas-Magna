using UnityEngine;


public abstract class CityProductionItem : ScriptableObject
{
    public string itemName;
    public float cost;
    public Sprite icon;

    public abstract void OnProductionComplete(City city);
}



[CreateAssetMenu(fileName = "Unit", menuName = "Scriptable Objects/ProductionItem/Unit")]
public class UnitProductionItem : CityProductionItem
{
    public GameObject unitPrefab;

    public override void OnProductionComplete(City city)
    {
        //Vector3 spawnPos = city.transform.position + Vector3.forward * 2f;
        //GameObject unit = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
        Debug.Log($"{city.cityName} a entraîné {itemName}");
    }
}



[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/ProductionItem/Building")]
public class BuildingProductionItem : CityProductionItem
{
    public string buildingID;
    public float bonusProduction;
    public float bonusFood;

    public override void OnProductionComplete(City city)
    {
        Debug.Log($"{city.cityName} a construit {itemName}");

        // Exemple : bonus appliqué à la ville
        //city.AddBuilding(this);
    }
}

