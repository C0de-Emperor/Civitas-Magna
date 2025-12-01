using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AI_Manager : MonoBehaviour
{
    public HexGrid grid;
    public bool isWorking = false;

    [HideInInspector] public Player AI_Player;

    [HideInInspector] public List<Unit> unactiveUnits;

    [Header("AI Parameters")]
    private int cityEvaluationRadius = 2;

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

        //unactiveUnits = data. ...
    }

    private void OnStartNewGame(NewGameData data)
    {
        AI_Player = data.AI_Player;

        unactiveUnits = new List<Unit>();
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

            unactiveUnits.Add(UnitManager.instance.AddUnit(settler, spawnCell, AI_Player));
            unactiveUnits.Add(UnitManager.instance.AddUnit(warrior, spawnCell, AI_Player));

            grid.UpdateActiveTiles();
        }
    }

    private void DoActions()
    {
        isWorking = true;

        ProcessUnactiveUnits();

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

        isWorking = false;
    }

    private void ProcessUnactiveUnits()
    {
        foreach (Unit unit in unactiveUnits)
        {
            if (unit.unitType is CivilianUnitType civil)
            {
                switch (civil.job)
                {
                    case CivilianUnitType.CivilianJob.Settler:
                        GiveOrderToSettler(civil, unit.unitTransform);
                        break;
                }
            }
        }
    }

    private void GiveOrderToSettler(CivilianUnitType settler, Transform tr)
    {
        HexCell bestCellForCity = GetBestCellForSettler();

        //UnitManager.instance.QueueUnitMovement(grid.GetTile(tr.position.x));
    }

    private HexCell GetBestCellForSettler()
    {
        float bestValue = float.MinValue;
        Vector2Int bestCoord = new Vector2Int(-9999, -9999);

        for (int y = 0; y < grid.gridSize.height; y++)
        {
            for (int x = 0; x < grid.gridSize.width; x++)
            {
                Vector2Int coord = new Vector2Int(x, y);

                if (CityManager.instance.tileToCity.ContainsKey(coord))
                    continue;

                if (grid.GetTile(coord) == null)
                    continue;

                if (!grid.GetTile(coord).terrainType.build.Contains(Building.BuildingNames.City))
                    continue;

                float value = EvaluateCellForCity(coord);

                if (value > bestValue)
                {
                    bestValue = value;
                    bestCoord = coord;
                }
            }
        }

        return (bestCoord.x >= 0)
            ? grid.GetTile(bestCoord)
            : null;
    }

    private float EvaluateCellForCity(Vector2Int offset)
    {
        HexCell center = grid.GetTile(offset);

        float score = 0f;

        Vector3 centerCube = HexMetrics.OffsetToCube(offset, grid.orientation);

        for (int dx = -cityEvaluationRadius; dx <= cityEvaluationRadius; dx++)
        {
            for (int dy = Mathf.Max(-cityEvaluationRadius, -dx - cityEvaluationRadius);
                     dy <= Mathf.Min(cityEvaluationRadius, -dx + cityEvaluationRadius);
                     dy++)
            {
                int dz = -dx - dy;

                Vector3 cube = new Vector3(
                    centerCube.x + dx,
                    centerCube.y + dy,
                    centerCube.z + dz
                );

                Vector2Int o = HexMetrics.CubeToOffset(cube, grid.orientation);

                HexCell cell = grid.GetTile(o);

                if (cell == null) continue;
                if (CityManager.instance.tileToCity.ContainsKey(o)) continue;

                score += cell.terrainType.food * 1.2f;
                score += cell.terrainType.production;
            }
        }

        return score;
    }

}
