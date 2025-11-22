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
            player = new Player(data.player.playerName, new Color[2] { data.player.livery.backgroundColor, data.player.livery.spriteColor });
            goldStock = data.goldStock;

            playerEntities = data.playerEntities.ToList();
        }
        else
        {
            player = new Player("bruh", new Color[2] { new Color(1, 1, 1), new Color(52f/255, 182f/255, 23f/255) });
            goldStock = 0f;

            playerEntities.Add(new Player("Barbarian", new Color[2] { new Color(1, 0, 0), new Color(0, 0, 0) }));
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
                if (!UnitManager.instance.IsCellTraversable(terrainType, settler) || !UnitManager.instance.IsCellTraversable(terrainType, warrior))
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

    public Player(string playerName, Color[] livery, bool isMainCharacter=false)
    {
        this.id = PlayerManager.instance.nextAvailableId;
        PlayerManager.instance.nextAvailableId++;

        this.playerName = playerName;
        this.livery.backgroundColor = livery[0];
        this.livery.spriteColor = livery[1];
    }
}

public struct Livery
{
    public Color backgroundColor;
    public Color spriteColor;
}