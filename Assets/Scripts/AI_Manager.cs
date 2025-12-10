using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI_Manager : MonoBehaviour
{
    [Header("References")]
    public HexGrid grid;

    [Header("Global")]
    [HideInInspector] public Player AI_Player;
    public AI_Diplomacy diplomacy;
    public bool isWorking = false;

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
    private int waitingTurn = 2;

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
        diplomacy = AI_Diplomacy.Neutral;
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
        if (TurnManager.instance.currentTurn <= waitingTurn)
            yield break;

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
        if (currentResearch == null)
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
            if (currentResearch != null)
                return;

            List<Research> availableResearches = new List<Research>();

            List<Research> toVisit = new List<Research>(sources);

            HashSet<Research> visitedSet = new HashSet<Research>();

            while (toVisit.Count > 0)
            {
                Research r = toVisit[0];
                toVisit.RemoveAt(0);

                if (visitedSet.Contains(r))
                    continue;

                visitedSet.Add(r);

                if (!researched.Contains(r))
                {
                    if (AreAllDependenciesResearched(r.dependencies))
                    {
                        availableResearches.Add(r);
                    }
                    else
                    {
                        foreach (Dependency dep in r.dependencies)
                        {
                            if (!researched.Contains(dep.research))
                                toVisit.Add(dep.research);
                        }
                    }
                }
            }

            if (availableResearches.Count > 0)
            {
                currentResearch = availableResearches[
                    UnityEngine.Random.Range(0, availableResearches.Count)
                ];

                AILog("New research started: " + currentResearch.researchName);
            }
        }
    }

    private bool AreAllDependenciesResearched(Dependency[] dependencies)
    {
        if (dependencies == null || dependencies.Length == 0)
            return true;

        foreach (var dep in dependencies)
            if (!researched.Contains(dep.research))
                return false;

        return true;
    }

    private void ProcessCities()
    {
        List<BuildingProductionItem> choosenBuildings = new List<BuildingProductionItem>();

        foreach(City city in cities)
        {
            /*
            faire une liste des choix fait par les autres villes,
            evaluer la situation de l'IA pour chaque production -> ici on ne considere pas une prod en batiment comme le joueur mais directement une augmentation d'une des stats de la ville
            => tenir compte des productions decidées de l'ia pour les autres villes a ce tour ci

            choisir la position avantageuse

             faire comme ça : 
            - avantage : on peut avoir une super profondeur d'arbre au tour par tour
            -desavantage : colle moins au gameplay du joueur


             */

            GetBestBuilding(choosenBuildings);

            //BuildingProductionItem i = new BuildingProductionItem
            //{
            //    ID = -1,
            //    costInGoldPerTurn = 0,

            //    bonusFood = 0,
            //    bonusGold = 0,
            //    bonusHealth = 0,
            //    bonusProduction = 0,
            //    bonusScience = 0,

            //    buildingRequierments = new List<BuildingProductionItem>()
            //};
        }
    }

    private void GetBestBuilding(List<BuildingProductionItem> choosenBuildings)
    {
        // generer un arbre alterné (player, AI)
    }

    private void EvaluateNode()
    {

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

        AILog($"Settler need to go to : {bestCellForCity.offsetCoordinates}");

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

    private void AILog(string msg)
    {
        Debug.Log($"<color=#00BBFF><b>[AI]</b></color> {msg}");
    }
}

public enum AI_Diplomacy {Neutral, Offensive}

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

[Serializable]
public class BuildingTree
{
    public Dictionary<BuildingTreeNode, List<BuildingTreeNode>> nodes;
}

[Serializable]
public class BuildingTreeNode
{
    public float score;
    public Player player;
    public BuildingProductionItem item;
}