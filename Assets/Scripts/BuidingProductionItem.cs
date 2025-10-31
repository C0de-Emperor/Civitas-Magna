using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/ProductionItem/Building")]
public class BuildingProductionItem : CityProductionItem
{
    public float bonusProduction;
    public float bonusFood;

    public override void OnProductionComplete(City city)
    {
        Debug.Log($"{city.cityName} a construit {itemName}");

        // Exemple : bonus appliqué à la ville
        //city.AddBuilding(this);
    }
}
