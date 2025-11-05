using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CityManager : MonoBehaviour
{
    [SerializeField] private Transform cityPrefab;
    [SerializeField] private Transform cityPanel;
    public City openedCity;

    [HideInInspector] public Dictionary<Vector2, City> cities = new Dictionary<Vector2, City>();

    [Header("UI")]
    [SerializeField] private Text cityName;

    [Header("Production")]
    [SerializeField] private Text food;
    [SerializeField] private Text production;
    [SerializeField] private Text gold;
    [SerializeField] private Text science;

    [Header("Categories")]
    [SerializeField] private Transform productionCat;
    [SerializeField] private Transform purchaseCat;

    [Header("Production View Ports")]
    [SerializeField] private ScrollRect productionBuildingsScroll;
    [SerializeField] private ScrollRect productionUnitsScroll;

    [Header("Purchase View Ports")]
    [SerializeField] private ScrollRect purchaseBuildingsScroll;
    [SerializeField] private ScrollRect purchaseUnitsScroll;

    [Header("Banner")]
    [SerializeField] private Canvas gameUICanvas;
    [SerializeField] private CityBannerUI cityBannerPrefab;



    private bool isProductionPanel = true;
    private Dictionary<Vector2, City> tileToCity = new Dictionary<Vector2, City>();

    [Tooltip("Liste des noms de villes disponibles (aucun doublon possible).")]
    private List<string> availableNames = new List<string>()
    {
        "Alexandria", "Babylon", "Carthage", "Thebes", "Sparta", "Corinth",
        "Rome", "Athens", "Byblos", "Uruk", "Nineveh", "Memphis",
        "Kyoto", "Tenochtitlan", "Cusco", "Lisbon", "Seville", "Venice",
        "Delhi", "Beijing", "Constantinople", "Córdoba", "Antioch", "Jericho",
        "Lyon", "Valence"
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
        component.occupiedCell = cell;

        cell.grid.RevealTilesInRadius(cell.offsetCoordinates, 3, SelectionManager.instance.showOverlay);
        cell.isACity = true;

        if(SelectionManager.instance.showOverlay)
        {
            component.HideForOverlay();
        }

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

        GameObject bannerObj = Instantiate(cityBannerPrefab.gameObject, gameUICanvas.transform);
        CityBannerUI banner = bannerObj.GetComponent<CityBannerUI>();

        banner.worldTarget = obj.transform;
        component.bannerUI = banner;

        component.UpdateBanner();
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
        CameraController camera = CameraController.instance;
        camera.canMove = false;
        camera.ChangeCamera(CameraMode.TopFocusCity);
        camera.SetTargetPosition(new Vector2(city.transform.position.x, city.transform.position.z));


        SelectionManager.instance.canInteract = false;  
        openedCity = city;

        cityName.text = openedCity.cityName;
        food.text = openedCity.GetCityFoodProduction().ToString();
        production.text = openedCity.GetCityProduction().ToString();
        gold.text = openedCity.GetCityGoldProduction().ToString();
        science.text = openedCity.GetCityScienceProduction().ToString();

        OpenProductionMenu();
        
        cityPanel.gameObject.SetActive(true);
    }

    internal void CloseCity()
    {
        if(openedCity == null)
        {
            Debug.LogError("There is no open city, you can always try to close it");
            return;
        }

        CameraController camera = CameraController.instance;
        camera.canMove = true;
        camera.ChangeCamera(CameraMode.Default);

        SelectionManager.instance.canInteract = true;
        openedCity = null;

        cityPanel.gameObject.SetActive(false);
    }

    public void UpdateAllBorders()
    {
        foreach(City city in cities.Values)
        {
            city.borders.UpdateBorders();
        }
    }

    public string GetRandomCityName()
    {
        if (availableNames.Count == 0)
            return "Unnamed";

        int index = UnityEngine.Random.Range(0, availableNames.Count);
        string chosenName = availableNames[index];
        availableNames.RemoveAt(index);
        return chosenName;
    }

    public void OpenBuildingsSubMenu()
    {
        if(isProductionPanel)
            BuildButtonManager.instance.RefreshUI(true);

        productionBuildingsScroll.gameObject.SetActive(isProductionPanel);
        purchaseBuildingsScroll.gameObject.SetActive(!isProductionPanel);

        purchaseUnitsScroll.gameObject.SetActive(false);
        productionUnitsScroll.gameObject.SetActive(false);

        // Remonte la ScrollView des bâtiments
        Canvas.ForceUpdateCanvases(); // important pour forcer le layout avant de modifier la position
        purchaseBuildingsScroll.verticalNormalizedPosition = 1f;
        productionBuildingsScroll.verticalNormalizedPosition = 1f;
    }

    public void OpenUnitsSubMenu()
    {
        if (isProductionPanel)
            BuildButtonManager.instance.RefreshUI(false);

        productionBuildingsScroll.gameObject.SetActive(false);
        purchaseBuildingsScroll.gameObject.SetActive(false);

        purchaseUnitsScroll.gameObject.SetActive(!isProductionPanel);
        productionUnitsScroll.gameObject.SetActive(isProductionPanel);

        // Remonte la ScrollView des unités
        Canvas.ForceUpdateCanvases();
        purchaseUnitsScroll.verticalNormalizedPosition = 1f;
        productionUnitsScroll.verticalNormalizedPosition = 1f;
    }

    public void OpenProductionMenu()
    {
        isProductionPanel = true;
        purchaseCat.gameObject.SetActive(false);
        productionCat.gameObject.SetActive(true);

        OpenBuildingsSubMenu();
    }

    public void OpenPurchaseMenu()
    {
        isProductionPanel = false;
        purchaseCat.gameObject.SetActive(true);
        productionCat.gameObject.SetActive(false);

        OpenBuildingsSubMenu();
    }

    public int PopulationFunction(int x)
    {
        return Mathf.RoundToInt( 0.11f * Mathf.Pow(x, 2f) + 10.2f * x + 2f);
    }
}