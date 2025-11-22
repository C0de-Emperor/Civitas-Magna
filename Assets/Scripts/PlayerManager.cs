using System;
using System.Collections.Generic;
using System.Linq;
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

        SaveManager.instance.OnSaveLoaded += OnLoad;
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

    public void OnLoad(SaveData data)
    {
        if(data != null)
        {
            goldStock = data.goldStock;

            foreach(var playerEntity in data.playerEntities)
            {
                playerEntities.Add(new Player(playerEntity.playerName, playerEntity.livery));
            }
            player = playerEntities[0];
        }
        else
        {
            player = new Player("bruh", new Livery( new Color(1, 1, 1), new Color(52f/255, 182f/255, 23f/255) ));
            playerEntities.Add(player);
            goldStock = 0f;

            playerEntities.Add(new Player("Barbarian", new Livery ( new Color(1, 0, 0), new Color(0, 0, 0) )));
        }
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
        }
    }
}

[Serializable]
public class Player
{
    public int id;
    public string playerName = "player";
    public Livery livery;

    public Player(string playerName, Livery livery)
    {
        this.id = PlayerManager.instance.nextAvailableId;
        PlayerManager.instance.nextAvailableId++;

        this.playerName = playerName;
        this.livery = livery;
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