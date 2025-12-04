using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI_Manager : MonoBehaviour
{
    public HexGrid grid;
    public bool isWorking = false;

    [HideInInspector] public Player AI_Player;

    [Header("Units")]
    [HideInInspector] public List<AIUnit> units = new List<AIUnit>();
    private List<Vector2Int> targetedPositions = new List<Vector2Int>();

    [Header("Cities")]
    [HideInInspector] public List<City> cities = new List<City>();

    [Header("Research")]
    private List<Research> sources = new List<Research>();
    public Research currentResearch;
    public float currentResearchProgress = 0f;
    public List<Research> researched = new List<Research>();

    [Header("Gold")]
    public float goldStock;

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
        TurnManager.instance.OnTurnChange += () => StartCoroutine(DoActions());

        // Trouver toutes les sources de notre graphe de recherche

        sources.Clear();
        sources = ResearchManager.instance.allResearches.ToList();

        foreach (Research research in ResearchManager.instance.allResearches)
        {
            foreach (Dependency dep in research.dependencies)
            {
                if(sources.Contains(dep.research))
                    sources.Remove(dep.research);
            }
        }
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
        goldStock = 0f;
    }

    internal void ResearchComplete()
    {
        researched.Add(currentResearch);

        currentResearch = null;
        currentResearchProgress = 0f;
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

            units.Add(
                new AIUnit 
                ( 
                    spawnCell,
                    UnitManager.instance.AddUnit(settler, spawnCell, AI_Player)
                ));
            units.Add(
                new AIUnit
                (
                    spawnCell,
                    UnitManager.instance.AddUnit(warrior, spawnCell, AI_Player) 
                ));

            grid.UpdateActiveTiles();
        }
    }

    private IEnumerator DoActions()
    {
        isWorking = true;

        yield return new WaitForSeconds(1f);

        ProcessUnits();

        ProcessCities();

        ProcessResearch();

        // update relation

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

    private void ProcessResearch()
    {
        if(currentResearch == null)
        {
            // trouver la prochaine recherche à realiser


            // la recherche X est en relation avec Y (X -> Y) si X a besoin que Y soit recherché pour être recherché
            /*
            D = on part des sources, c'est à dire les recherches qui ne debloque rien.

            on prend un element A de D,


            si A n'est pas recherché
            
                si toutes les dependances de A sont recherché :
                    on ajoute A à la liste des recherches possibles
                
                sinon
                    on ajoute les dependances de A a D


            si A recherché
                rien
            */
            List<Research> availableResearches = new List<Research>();
            List<Research> toVisit = new List<Research>();
            toVisit = sources;

            while (toVisit.Count > 0)
            {
                Research visited = toVisit[0];
                toVisit.RemoveAt(0);

                if (!researched.Contains(visited))
                {
                    if (AreAllDependenciesResearched(visited.dependencies))
                    {
                        availableResearches.Add(visited);
                    }
                    else
                    {
                        foreach (Dependency dep in visited.dependencies)
                        {
                            if(!researched.Contains(dep.research))
                                toVisit.Add(dep.research);
                        }
                    }
                }
            }

            if(availableResearches.Count > 0)
                currentResearch = availableResearches[0];
        }
    }

    private bool AreAllDependenciesResearched(Dependency[] dependencies)
    {
        foreach (Dependency dependency in dependencies)
        {
            if (!researched.Contains(dependency.research))
            {
                return false;
            }
        }

        return true;
    }

    private void ProcessCities()
    {
        foreach(City city in cities)
        {
            // assigner la production
        }
    }

    private void ProcessUnits() // differencier les actives des inactives ; inactives = nouvel ordre ; active = verification de l'objectif et control
    {
        foreach (AIUnit AIUnit in units)
        {
            if (IsUnitInactive(AIUnit.unit))
            {
                if (AIUnit.unit.unitType is CivilianUnitType civil)
                {
                    switch (civil.job)
                    {
                        case CivilianUnitType.CivilianJob.Settler:
                            GiveOrderToSettler(civil, AIUnit.cell, AIUnit.unit.master);
                            break;
                    }
                }
            }
        }
    }

    private void GiveOrderToSettler(CivilianUnitType settler, HexCell position, Player master)
    {
        HexCell bestCellForCity = GetBestCellForSettler(position);
        targetedPositions.Add(bestCellForCity.offsetCoordinates);

        Debug.Log("need to go to : " + bestCellForCity.offsetCoordinates);

        UnitManager.instance.QueueUnitMovement(
            position, 
            bestCellForCity, 
            UnitType.UnitCategory.civilian, 
            () =>
            {
                UnitManager.instance.CivilianUnitAction(bestCellForCity, Building.BuildingNames.City);
                targetedPositions.Remove(bestCellForCity.offsetCoordinates);
            }, 
            true
        );

    }

    private bool IsUnitInactive(Unit unit)
    {
        return !UnitManager.instance.queuedUnitMovements.ContainsKey(unit.id);
    }

    private HexCell GetBestCellForSettler(HexCell position)
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

                if (UnitManager.instance.GetShortestPath(position, grid.GetTile(coord), UnitManager.instance.GetUnitType("Settler")) == null)
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

    public void RemoveAIUnit(Unit unit)
    {
        foreach(AIUnit AIUnit in units)
        {
            if(AIUnit.unit == unit)
            {
                units.Remove(AIUnit);
                return;
            }
        }
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
                if (targetedPositions.Contains(cell.offsetCoordinates)) return -1;

                if(cell.building != null)
                {
                    score += cell.terrainType.food * 1.2f * cell.building.foodFactor;
                    score += cell.terrainType.production * cell.building.productionFactor;
                }
                else
                {
                    score += cell.terrainType.food * 1.2f;
                    score += cell.terrainType.production;
                }
            }
        }

        return score;
    }
}

[Serializable]
public class AIUnit
{
    public HexCell cell;
    public Unit unit;

    public AIUnit(HexCell cell, Unit unit)
    {
        this.cell = cell;
        this.unit = unit;
    }
}
