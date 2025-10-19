using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private Transform cityPrefab;

    public Dictionary<Vector2, City> cities = new Dictionary<Vector2, City>();

    Dictionary<Vector2, City> tileToCity = new Dictionary<Vector2, City>();

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
        if (cell == null 
            || cell.ressource != null 
            || cell.isActive == false 
            || cell.isRevealed == false 
            || !cell.terrainType.build.Contains(TerrainType.Build.City)
            || IsToACity(cell)
            )
        {
            return;
        }
            

        Transform obj = cell.InstantiateRessource(cityPrefab);

        City component = obj.GetComponent<City>();
        if(component == null)
        {
            Debug.LogError("City Prefab ne convient pas, il manque un City component");
            return;
        }

        cell.grid.RevealTilesInRadius(cell.offsetCoordinates, 3);
        cell.isACity = true;

        cities.Add(cell.offsetCoordinates, component);

        foreach(HexCell n in cell.neighbours)
        {
            if(n != null && !tileToCity.ContainsKey(n.offsetCoordinates))
            {
                tileToCity.Add(n.offsetCoordinates, component);
                component.controlledTiles.Add(n.offsetCoordinates, n);
            }
        }
        component.controlledTiles.Add(cell.offsetCoordinates, cell);
        tileToCity.Add(cell.offsetCoordinates, component);

        UpdateAllBorders();
    }

    public bool IsToACity(HexCell cell)
    {
        return (cell == null)? false : tileToCity.ContainsKey(cell.offsetCoordinates);
    }

    public void UpdateAllBorders()
    {
        foreach(City city in cities.Values)
        {
            city.borders.UpdateBorders();
        }
    }
}
