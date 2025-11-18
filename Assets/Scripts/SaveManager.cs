using UnityEngine;
using System.IO;
using System;

public class SaveManager : MonoBehaviour
{
    public bool canSave;
    public bool isWorking = false;

    [field:NonSerialized] public SaveData lastSave { get; private set; }

    public bool hasLoaded = false;

    public static SaveManager instance;

    private string savePath;

    public event Action<SaveData> OnSaveLoaded;

    private void Awake()
    {
        canSave = false;

        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        savePath = Path.Combine(Application.persistentDataPath, "save.json");

        DontDestroyOnLoad(gameObject);
    }

    public void TriggerSaveLoaded(SaveData data)
    {
        canSave = true;
        lastSave = data;
        hasLoaded = true;
        OnSaveLoaded?.Invoke(data);
    }

    public void ClearAllSaveLoadedSubscribers()
    {
        OnSaveLoaded = null;
    }

    public void SaveData()
    {
        isWorking = true;

        try
        {
            HexGrid grid = SelectionManager.instance.grid;
            CityManager cityM = CityManager.instance;
            ResearchManager researchM = ResearchManager.instance;
            UnitManager unitM = UnitManager.instance;
            PlayerManager playerM = PlayerManager.instance;

            SaveData data = new SaveData
            {
                creationTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),

                player = playerM.player,
                goldStock = playerM.goldStock,

                currentTurn = TurnManager.instance.currentTurn,

                seed = MapGenerator.instance.seed,
                orientation = grid.orientation,
                width = grid.width,
                height = grid.height,
                hexSize = grid.hexSize,
                batchSize = grid.batchSize,
                cells = grid.GetAllCellData(),

                maxCityRadius = cityM.maxCityRadius,
                availableNames = cityM.availableNames.ToArray(),
                cities = CityManager.instance.GetAllCityData(),

                currentResearch = researchM.currentResearch,
                currentResearchProgress = researchM.currentResearchProgress,
                researched = researchM.researched.ToArray(),

                nextAvailableId = unitM.nextAvailableId,
                units = unitM.GetAllUnitData()
            };

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);

            Debug.Log("Game Saved !");

        }
        catch (Exception e)
        {
            throw new Exception("Erreur lors de la sauvegarde des données", e);
        }
        isWorking = false;
    }

    public SaveData LoadData()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No save file found!");
            return null;
        }

        string json = File.ReadAllText(savePath);
        return JsonUtility.FromJson<SaveData>(json);
    }
}

[Serializable]
public class SaveData
{
    [Header("General")]
    public string creationTime;

    [Header("Player")]
    public Player player;
    public float goldStock;

    [Header("Turn")]
    public int currentTurn;

    [Header("Grid")]
    public int seed;
    public HexOrientation orientation;
    public int width;
    public int height;
    public float hexSize;
    public int batchSize;
    public HexCellData[] cells;

    [Header("City")]
    public int maxCityRadius;
    public string[] availableNames;
    public CityData[] cities;

    [Header("Research")]
    public Research currentResearch;
    public float currentResearchProgress;
    public Research[] researched;

    [Header("Units")]
    public int nextAvailableId;
    public UnitData[] units;
}

[Serializable]
public class HexCellData
{
    [Header("Terrain")]
    public int terrainTypeID;
    public float terrainHigh;

    [Header("Position")]
    public Vector2Int offsetCoordinates;

    [Header("Properties")]
    public bool isRevealed;
    public bool isActive;
    public Building building;
}

[Serializable]
public class CityData
{
    [Header("General")]
    public string cityName;
    public Player master;

    [Header("Position")]
    public Vector2Int offsetCoordinates;

    [Header("Population")]
    public int population;
    public float foodStock;

    [Header("Health")]
    public float damage;
    public float baseHealth;

    [Header("Production")]
    public CityProductionItem currentProduction;
    public float currentProductionProgress;
    public BuildingProductionItem[] builtBuildings;
    public float cityFactor;

    [Header("Controlled Tiles")]
    public Vector2Int[] controlledTilesOffsetsCoordinates;
}

[Serializable]
public class UnitData
{
    public int id;
    public Vector3 position;
    public UnitType unitType;
    public Player master;
    public string unitName;

    public float currentHealth;

    public float movesDone;
    public int lastDamagingTurn;

    public queuedMovementData queuedMovementData;

}

/*
 * player
 * turn - Done !
 * tiles - Done !
 * city - Done !
 * units - Done !
 * research - Done !
*/