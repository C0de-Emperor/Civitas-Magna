using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public int nextAvailableId = 0;
    public Player player;
    public HexGrid grid;

    public List<Player> playerEntities = new List<Player>();

    [Header("UI")]
    [SerializeField] private Text goldStockText;

    [HideInInspector] public float goldStock;

    public static PlayerManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de PlayerManager dans la scène");
            return;
        }
        instance = this;

        SaveManager.instance.OnSaveLoaded += OnLoadSave;
        SaveManager.instance.OnNewGameStarted += OnStartNewGame;
        grid.OnCellInstancesGenerated += OnCellLoaded;
        UpdateMainUI();
    }

    private void Start()
    {
        TurnManager.instance.OnTurnChange += UpdateMainUI;
    }

    private void UpdateMainUI()
    {
        goldStockText.text = Mathf.RoundToInt(goldStock).ToString();
    }

    private void OnLoadSave(SaveData data)
    {
        if (data == null)
            throw new Exception("SaveData is null");


        goldStock = data.goldStock;

        foreach(var playerEntity in data.playerEntities)
        {
            playerEntities.Add(new Player(playerEntity.playerName, playerEntity.livery));
        }
        player = playerEntities[0];
    }

    private void OnStartNewGame(NewGameData data)
    {
        player = data.player;
        playerEntities.Add(player);
        goldStock = 0f;

        playerEntities.Add(new Player("Barbarian", new Livery(new Color(1, 0, 0), new Color(0, 0, 0))));
    }

    public void OnCellLoaded()
    {
        if(SaveManager.instance.lastSave == null)
        {
            UnitType settler = UnitManager.instance.GetUnitType("Settler");
            UnitType warrior = UnitManager.instance.GetUnitType("Warrior");

            List<TerrainType> forbiddenTerrainTypes = new List<TerrainType>();
            foreach(var terrainType in grid.terrainTypes)
            {
                if (!UnitManager.instance.IsTerrainTypeTraversable(terrainType, settler) || !UnitManager.instance.IsTerrainTypeTraversable(terrainType, warrior))
                {
                    forbiddenTerrainTypes.Add(terrainType);
                }
            }

            HexCell spawnCell = grid.GetRandomCell(false, forbiddenTerrainTypes);

            UnitManager.instance.AddUnit(settler, spawnCell, player);
            UnitManager.instance.AddUnit(warrior, spawnCell, player);

            CameraController.instance.SetTargetPosition(new Vector2(spawnCell.tile.position.x, spawnCell.tile.position.z));

            grid.UpdateActiveTiles();
        }
    }
}

[Serializable]
public class Player
{
    public int id;
    public string playerName = "player";
    public Livery livery;

    public float combatPower;
    public float sciencePower;
    public float expansionPower;
    public float ressourcesPower;

    public Player(string playerName, Livery livery)
    {
        this.id = PlayerManager.instance.nextAvailableId;
        PlayerManager.instance.nextAvailableId++;

        this.playerName = playerName;
        this.livery = livery;
    }

    public void GetCombatPower()
    {
        this.combatPower = 0;
        foreach (var unit in UnitManager.instance.units.Values)
        {
            if(unit.unitType.unitCategory == UnitType.UnitCategory.military && unit.master == this)
            {
                this.combatPower += unit.GetUnitMilitaryData().militaryPower;
            }
        }
    }

    public void GetExpansionPower()
    {
        this.expansionPower = 0;
        this.ressourcesPower = 0;
        foreach(var city in CityManager.instance.cities.Values)
        {
            if (city.master == this)
            {
                this.expansionPower += city.controlledTiles.Count * 1 + city.population * 1;
                this.ressourcesPower += city.GetCityFoodProduction() * 1 + city.GetCityGoldProduction() * 1 + city.GetCityProduction() * 1;
            }
        }
    }

    public void GetSciencePower()
    {
        this.sciencePower = 0;
        foreach(var research in ResearchManager.instance.researched)
        {
            this.sciencePower += research.researchValue;
        }
    }
}

[Serializable]
public class Livery
{
    public Color backgroundColor;
    public Color spriteColor;

    public Livery(Color backgroundColor, Color spriteColor)
    {
        this.backgroundColor = backgroundColor;
        this.spriteColor = spriteColor;
    }
}