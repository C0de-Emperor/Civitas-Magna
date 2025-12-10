using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "Scriptable Objects/ProductionItem/Unit")]
public class UnitProductionItem : CityProductionItem
{
    public UnitType unit;

    public override void OnProductionComplete(City city)
    {
        UnitManager.instance.AddUnit(unit, city.occupiedCell, city.master);

        Debug.Log($"{city.cityName} a entraîné {itemName}");
    }
}
