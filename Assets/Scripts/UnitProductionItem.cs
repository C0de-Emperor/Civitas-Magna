using UnityEngine;

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
