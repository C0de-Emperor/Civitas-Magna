using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CityManager : MonoBehaviour
{
    [SerializeField] private Transform cityPrefab;
    [SerializeField] private Transform cityPanel;

    [HideInInspector] public Dictionary<Vector2, City> cities = new Dictionary<Vector2, City>();

    [Header("UI")]
    [SerializeField] private Text cityName;
    [Header("Production")]
    [SerializeField] private Text food;
    [SerializeField] private Text production;
    [SerializeField] private Text gold;
    [SerializeField] private Text science;
    [Header("View Ports")]
    [SerializeField] private Transform buildingsViewPort;
    [SerializeField] private Transform unitsViewPort;

    [SerializeField] private ScrollRect buildingsScroll;
    [SerializeField] private ScrollRect unitsScroll;
    [Header("Buttons")]
    [SerializeField] private Button unitsProductionButton;



    private Dictionary<Vector2, City> tileToCity = new Dictionary<Vector2, City>();
    private City openedCity;

    [Tooltip("Liste des noms de villes disponibles (aucun doublon possible).")]
    private List<string> availableNames = new List<string>()
    {
        "Alexandria", "Babylon", "Carthage", "Thebes", "Sparta", "Corinth",
        "Rome", "Athens", "Byblos", "Uruk", "Nineveh", "Memphis",
        "Kyoto", "Tenochtitlan", "Cusco", "Lisbon", "Seville", "Venice",
        "Delhi", "Beijing", "Constantinople", "Córdoba", "Antioch", "Jericho"
    };

    public static CityManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de BuildingManager dans la scène");
            return;
        }
        instance = this;

        cityPanel.gameObject.SetActive(false);
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
        component.cityName = GetRandomCityName();
        component.controlledTiles.Add(cell.offsetCoordinates, cell);
        tileToCity.Add(cell.offsetCoordinates, component);

        UpdateAllBorders();
    }

    public bool IsToACity(HexCell cell)
    {
        return (cell == null)? false : tileToCity.ContainsKey(cell.offsetCoordinates);
    }

    internal void OpenCity(City city)
    {
        if(openedCity != null)
        {
            return;
        }

        CameraController.instance.canMove = false;
        SelectionManager.instance.canInteract = false;  
        openedCity = city;

        cityName.text = openedCity.cityName;
        food.text = openedCity.food.ToString();
        production.text = openedCity.production.ToString();
        gold.text = openedCity.gold.ToString();
        science.text = openedCity.science.ToString();




        cityPanel.gameObject.SetActive(true);
        OpenProductionBuildingsMenu();
    }

    public void UpdateAllBorders()
    {
        foreach(City city in cities.Values)
        {
            city.borders.UpdateBorders();
        }
    }

    /// <summary>
    /// Donne un nom de ville aléatoire unique. Retourne "Unnamed" si plus de noms disponibles.
    /// </summary>
    public string GetRandomCityName()
    {
        if (availableNames.Count == 0)
            return "Unnamed";

        int index = UnityEngine.Random.Range(0, availableNames.Count);
        string chosenName = availableNames[index];
        availableNames.RemoveAt(index);
        return chosenName;
    }

    public void OpenProductionBuildingsMenu()
    {
        buildingsViewPort.gameObject.SetActive(true);
        unitsViewPort.gameObject.SetActive(false);

        // Remonte la ScrollView des bâtiments
        Canvas.ForceUpdateCanvases(); // important pour forcer le layout avant de modifier la position
        buildingsScroll.verticalNormalizedPosition = 1f;
    }

    public void OpenProductionUnitsMenu()
    {
        buildingsViewPort.gameObject.SetActive(false);
        unitsViewPort.gameObject.SetActive(true);

        // Remonte la ScrollView des unités
        Canvas.ForceUpdateCanvases();
        unitsScroll.verticalNormalizedPosition = 1f;
    }


    //not yet

    public void OpenProductionMenu()
    {

    }

    public void OpenPurchaseMenu()
    {

    }

    public void OpenPurchaseBuildingsMenu()
    {

    }

    public void OpenPurchaseUnitsMenu()
    {

    }





}
