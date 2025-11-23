using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class CityManager : MonoBehaviour
{
    [SerializeField] private HexGrid grid;

    [SerializeField] private Transform cityPrefab;
    [SerializeField] private Transform cityPanel;
    public City openedCity;
    public int maxCityRadius;

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
    [SerializeField] public Dictionary<Vector2, City> tileToCity = new Dictionary<Vector2, City>();

    [Tooltip("Liste des noms de villes disponibles")]
    [HideInInspector] public List<string> availableNames;

    public static CityManager instance;
    private void Awake()
    {
        grid.OnCellInstancesGenerated += OnLoadSave;
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de BuildingManager dans la scène");
            return;
        }
        instance = this;

        cityPanel.gameObject.SetActive(false);
    }

    private void Start()
    {
        TurnManager.instance.OnTurnChange += ProcessQueuedExpansions;
    }

    private void OnLoadSave()
    {
        SaveData data = SaveManager.instance.lastSave;

        if (data == null)
            return;


        availableNames = data.availableNames.ToList();
        maxCityRadius = data.maxCityRadius;

        foreach (CityData cityData in data.cities)
        {
            CreateCityFromSave(cityData);
        }
    }

    private void OnStartNewGame(NewGameData data)
    {
        availableNames = new List<string>() { "Kabul", "Tirana", "Algiers", "Pago Pago", "Andorra la Vella", "Luanda", "The Valley",
                "St. John's", "Buenos Aires", "Yerevan", "Oranjestad", "Canberra", "Vienna", "Baku", "Nassau", "Manama", "Dhaka",
                "Bridgetown", "Minsk", "Bruxelles-Brussel", "Belmopan", "Cotonou", "Hamilton", "Thimphu", "La Paz", "Sarajevo",
                "Gaborone", "Brasília", "Road Town", "Bandar Seri Begawan", "Sofia", "Ouagadougou", "Bujumbura", "Praia",
                "Phnom Penh", "Yaoundé", "Ottawa-Gatineau", "Kralendijk", "George Town", "Bangui", "N'Djaména", "St. Helier",
                "St. Peter Port", "Santiago", "Beijing", " Hong Kong SAR'", " Macao SAR'", " Taiwan Province of China'", "Bogotá",
                "Moroni", "Brazzaville", "Rarotonga", "San José", "Abidjan", "Zagreb", "Havana", "Willemstad", "Nicosia", "Prague",
                "P'yongyang", "Kinshasa", "Copenhagen", "Djibouti", "Roseau", "Santo Domingo", "Quito", "Cairo", "San Salvador",
                "Malabo", "Asmara", "Tallinn", "Addis Ababa", "Tórshavn", "Stanley", "Suva", "Helsinki", "Paris", "Cayenne",
                "Papeete", "Libreville", "Banjul", "Tbilisi", "Berlin", "Accra", "Gibraltar", "Athens", "Godthåb", "St.George's",
                "Basse-Terre", "Hagåtña", "Guatemala City", "Conakry", "Bissau", "Georgetown", "Port-au-Prince", "Vatican City",
                "Tegucigalpa", "Budapest", "Reykjavík", "Delhi", "Jakarta", "Tehran", "Baghdad", "Dublin", "Douglas", "Jerusalem",
                "Rome", "Kingston", "Tokyo", "Amman", "Astana", "Nairobi", "Tarawa", "Kuwait City", "Bishkek", "Vientiane", "Riga",
                "Beirut", "Maseru", "Monrovia", "Tripoli", "Vaduz", "Vilnius", "Luxembourg", "Antananarivo", "Lilongwe", "Kuala Lumpur",
                "Male", "Bamako", "Valletta", "Majuro", "Fort-de-France", "Nouakchott", "Port Louis", "Mamoudzou", "Mexico City", "Palikir",
                "Monaco", "Ulaanbaatar", "Podgorica", "Brades Estate", "Rabat", "Maputo", "Nay Pyi Taw", "Windhoek", "Nauru", "Kathmandu",
                "Amsterdam", "Nouméa", "Wellington", "Managua", "Niamey", "Abuja", "Alofi", "Saipan", "Oslo", "Muscat", "Islamabad", "Koror",
                "Panama City", "Port Moresby", "Asunción", "Lima", "Manila", "Warsaw", "Lisbon", "San Juan", "Doha", "Seoul", "Chişinău",
                "Saint-Denis", "Bucharest", "Moscow", "Kigali", "Jamestown", "Basseterre", "Castries", "Saint-Pierre", "Kingstown", "Apia",
                "San Marino", "São Tomé", "Riyadh", "Dakar", "Belgrade", "Victoria", "Freetown", "Singapore", "Philipsburg", "Bratislava",
                "Ljubljana", "Honiara", "Mogadishu", "Cape Town", "Juba", "Madrid", "Colombo", "Al-Quds[East Jerusalem]", "Khartoum",
                "Paramaribo", "Mbabane", "Stockholm", "Bern", "Damascus", "Dushanbe", "Skopje", "Bangkok", "Dili", "Lomé", "Tokelau", "Nuku'alofa",
                "Port of Spain", "Tunis", "Ankara", "Ashgabat", "Cockburn Town", "Funafuti", "Kampala", "Kiev", "Abu Dhabi", "London", "Dodoma",
                "'Washington", "Charlotte Amalie", "Montevideo", "Tashkent", "Port Vila", "Caracas", "Hà Noi", "Matu-Utu", "El Aaiún", "Sana'a'",
                "Lusaka", "Harare" };
        maxCityRadius = 4;
    }

    public bool CreateCity(HexCell cell, Player master)
    {
        if (cell == null 
            || cell.ressource != null 
            || cell.isActive == false 
            || cell.isRevealed == false 
            || !cell.terrainType.build.Contains(Building.BuildingNames.City)
            || IsToACity(cell)
            )
        {
            return false;
        }

        Transform obj = cell.InstantiateRessource(cityPrefab);

        City component = obj.GetComponent<City>();
        component.master = master;
        if(component == null)
        {
            Debug.LogError("City Prefab ne convient pas, il manque un City component");
            return false;
        }
        component.occupiedCell = cell;

        grid.RevealTilesInRadius(cell.offsetCoordinates, 3, SelectionManager.instance.showOverlay, true);

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
        banner.transform.GetChild(0).GetComponent<Image>().color = master.livery.backgroundColor;
        banner.transform.GetChild(1).GetComponent<Text>().color = master.livery.spriteColor;
        component.bannerUI = banner;

        component.UpdateBanner();

        return true;
    }

    private void CreateCityFromSave(CityData data)
    {
        HexCell occupiedCell = grid.GetTile(data.offsetCoordinates);

        Transform obj =  occupiedCell.InstantiateRessource(cityPrefab);

        City component = obj.GetComponent<City>();
        component.master = data.master;
        if (component == null)
        {
            Debug.LogError("City Prefab ne convient pas, il manque un City component");
            return;
        }
        component.occupiedCell = occupiedCell;

        grid.RevealTilesInRadius(occupiedCell.offsetCoordinates, 3, SelectionManager.instance.showOverlay, true);

        if (SelectionManager.instance.showOverlay)
        {
            component.HideForOverlay();
        }


        cities.Add(data.offsetCoordinates, component);
        foreach(Vector2Int pos in data.controlledTilesOffsetsCoordinates)
        {
            tileToCity.Add(pos, component);
            component.controlledTiles.Add(pos, grid.GetTile(pos));
        }

        component.cityName = data.cityName;
        component.population = data.population;
        component.foodStock = data.foodStock;
        component.damage = data.damage;
        component.baseHealth = data.baseHealth;
        component.currentProduction = data.currentProduction;
        component.currentProductionProgress = data.currentProductionProgress;
        component.builtBuildings = data.builtBuildings.ToList();
        component.cityFactor = data.cityFactor;

        UpdateAllBorders();

        GameObject bannerObj = Instantiate(cityBannerPrefab.gameObject, gameUICanvas.transform);
        CityBannerUI banner = bannerObj.GetComponent<CityBannerUI>();

        banner.worldTarget = obj.transform;
        banner.transform.GetChild(0).GetComponent<Image>().color = data.master.livery.backgroundColor;
        banner.transform.GetChild(1).GetComponent<Text>().color = data.master.livery.spriteColor;
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

        Canvas.ForceUpdateCanvases();
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
        return Mathf.RoundToInt( 0.12f * Mathf.Pow(x, 2f) + 11.2f * x + 3f);
    }

    public int GetTurnsToProduce(CityProductionItem item, City city)
    {
        float prodRequired = 0f;
        if (city.currentProduction == item)
            prodRequired = item.costInProduction - city.currentProductionProgress;
        else
            prodRequired = item.costInProduction;
        float net = city.GetCityProduction();

        return Mathf.CeilToInt(prodRequired / net);
    }

    public float GetTotalScienceProduction()
    {
        float amount = 0f;

        foreach(City city in cities.Values)
        {
            amount += city.GetCityScienceProduction();
        }

        return amount;
    }

    private List<City> pendingExpansions = new List<City>();

    public void QueueCityExpansion(City city)
    {
        if (!pendingExpansions.Contains(city))
            pendingExpansions.Add(city);
    }

    private void ProcessQueuedExpansions()
    {
        // Étendre toutes les villes
        foreach (var city in pendingExpansions)
        {
            city.borders.ExpandCity();
        }

        // Maj des frontières
        foreach (var city in cities.Values)
        {
            city.borders.UpdateBorders();
        }

        pendingExpansions.Clear();
    }

    public CityData[] GetAllCityData()
    {
        CityData[] citiesData = new CityData[cities.Count];

        int i = 0;

        foreach (City city in cities.Values)
        {
            citiesData[i] = new CityData
            {
                cityName = city.cityName,
                master = city.master,

                offsetCoordinates = city.occupiedCell.offsetCoordinates,

                population = city.population,
                foodStock = city.foodStock,

                damage = city.damage,
                baseHealth = city.baseHealth,


                currentProduction = city.currentProduction,
                currentProductionProgress = city.currentProductionProgress,
                builtBuildings = city.builtBuildings.ToArray(),
                cityFactor = city.cityFactor,

                controlledTilesOffsetsCoordinates = city.controlledTiles.Keys.ToArray()
            };
            i++;
        }

        return citiesData;
    }
}