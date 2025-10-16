using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private Transform cityPrefab;

    [SerializeField] private Dictionary<Vector2, City> cities = new Dictionary<Vector2, City>();

    public static BuildingManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de BuildingManager dans la scène");
            return;
        }
        instance = this;
    }

    public void CreateCity(HexCell cell)
    {
        if (cell == null || cell.ressource != null || cell.isActive == false || cell.isRevealed == false)
            return;

        Transform obj = cell.InstantiateRessource(cityPrefab);

        City component = obj.GetComponent<City>();
        if(component == null)
        {
            Debug.LogError("City Prefab ne convient pas, il manque un City component");
            return;
        }

        cities.Add(cell.offsetCoordinates, component);
    }
}
