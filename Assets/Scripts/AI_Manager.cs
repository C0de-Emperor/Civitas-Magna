using System;
using System.Collections.Generic;
using UnityEngine;

public class AI_Manager : MonoBehaviour
{
    public HexGrid grid;
    [HideInInspector] public Player AI_Player;

    public static AI_Manager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de AI_Manager dans la scène");
            return;
        }
        instance = this;

        grid.OnCellInstancesGenerated += OnCellLoaded;
        SaveManager.instance.OnSaveLoaded += OnLoadSave;
        SaveManager.instance.OnNewGameStarted += OnStartNewGame;
    }

    private void Start()
    {
        TurnManager.instance.OnTurnChange += DoActions;
    }

    private void OnLoadSave(SaveData data)
    {
        if (data == null)
            throw new Exception("SaveData is null");
    }


    private void OnStartNewGame(NewGameData data)
    {
        AI_Player = data.AI_Player;
    }


    public void OnCellLoaded()
    {
        if (SaveManager.instance.lastSave == null)
        {
            UnitType settler = UnitManager.instance.GetUnitType("Settler");
            UnitType warrior = UnitManager.instance.GetUnitType("Warrior");

            List<TerrainType> forbiddenTerrainTypes = new List<TerrainType>();
            foreach (var terrainType in grid.terrainTypes)
            {
                if (!UnitManager.instance.IsTerrainTypeTraversable(terrainType, settler) || !UnitManager.instance.IsTerrainTypeTraversable(terrainType, warrior))
                {
                    forbiddenTerrainTypes.Add(terrainType);
                }
            }

            HexCell spawnCell = grid.GetRandomCell(false, forbiddenTerrainTypes);

            UnitManager.instance.AddUnit(settler, spawnCell, AI_Player);
            UnitManager.instance.AddUnit(warrior, spawnCell, AI_Player);

            grid.UpdateActiveTiles();
        }
    }

    private void DoActions()
    {
        /*
        Get All Informations
        -------------
        research power
        combat power
        expantion
        amenagement

        -------------

        Start Research

        Set All City Production

        Move All Unit
        */
    }
}
