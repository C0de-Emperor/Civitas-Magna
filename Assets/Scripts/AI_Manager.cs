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
    public int turnsSinceOffensive;
    public bool isWorking = false;
    public int nonAgressionTurnsNumber = 10;
    public Transform aiWonText;

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
    private int maxSettlerAount = 1;

    private float expansionFactor = 1f;
    private float combatFactor = 1f;
    private float scienceFactor = 1f;
    private float ressourceFactor = 1f;
    [Range(0f, 1f)] private float advantageFactor = 0.75f;

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
                if (!UnitManager.instance.IsTerrainTypeTraversable(terrainType, settler, researched.Contains(UnitManager.instance.canBoatResearch)) || !UnitManager.instance.IsTerrainTypeTraversable(terrainType, warrior, researched.Contains(UnitManager.instance.canBoatResearch)))
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
        if(TurnManager.instance.currentTurn <= waitingTurn)
        {
            return;
        }

        isWorking = true;

        ProcessDiplomacy();

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
        List<CityProductionItem> choosenItems = new List<CityProductionItem>();

        

        foreach (City city in cities)
        {
            if (city.currentProduction != null)
                continue;

            CityProductionItem item = GetBestProductionItem(choosenItems);
            choosenItems.Add(item);

            
            city.SetProduction(item);
        }
    }

    private CityProductionItem GetBestProductionItem(List<CityProductionItem> choosenItems)
    {
        Player player = PlayerManager.instance.player;

        if (AI_Player.expansionPower <= player.expansionPower * expansionFactor)
        {
            // determiner me nombre de settler en production
            int settlerAmount = 0;
            foreach(CityProductionItem item in choosenItems)
            {
                if(item is UnitProductionItem unitItem)
                {
                    if(unitItem.unit is CivilianUnitType civilianUnit)
                    {
                        if (civilianUnit.job == CivilianUnitType.CivilianJob.Settler)
                            settlerAmount++;
                    }
                }
            }

            // compter les settler présent sur le plateau
            foreach(AIUnit AIUnit in units)
            {
                if (AIUnit.unit.unitType is CivilianUnitType civilianUnit)
                {
                    if (civilianUnit.job == CivilianUnitType.CivilianJob.Settler)
                        settlerAmount++;
                }
            }

            if(settlerAmount <= maxSettlerAount)
            {
                // on produit un settler
                AILog("Production d'un Settler");
                return BuildButtonManager.instance.GetSettlerProductionItem();
            }
            else
            {
                // on produit un batiment renforçant la production de nourriture
                AILog("Production d'un Building");
                return CreateNewItem(
                
                    UnityEngine.Random.Range(0.1f, 0.6f),
                    0,
                    0,
                    0,
                    0,
                    10
                );
            }
        }

        if (AI_Player.combatPower <= player.combatPower * combatFactor)
        {
            // plus il y a d'unité en cours de prod plus on baisse la proba de lancer une nouvelle unit
            // determiner me nombre d'unité de combat en production
            int militaryUnitAmount = 0;
            foreach (CityProductionItem item in choosenItems)
            {
                if (item is UnitProductionItem unitItem)
                {
                    if (unitItem.unit is MilitaryUnitType)
                    {
                        militaryUnitAmount++;
                    }
                }
            }

            if(militaryUnitAmount > 0)
            {
                if(UnityEngine.Random.Range(0f, 1f) <= 1 / militaryUnitAmount)
                {
                    AILog("Production d'une Unité Militaire");
                    return BuildButtonManager.instance.GetRandomMilitaryUnit();
                }
            }
            else
            {
                AILog("Production d'une Unité Militaire");
                return BuildButtonManager.instance.GetRandomMilitaryUnit();
            }
        }

        if(AI_Player.sciencePower <= player.sciencePower * scienceFactor)
        {
            AILog("Production d'un Building Scientifique");
            return CreateNewItem(
                0,
                0,
                0,
                0,
                UnityEngine.Random.Range(0.1f, 0.3f),
                10
            );
        }

        if (AI_Player.ressourcesPower <= player.ressourcesPower * ressourceFactor)
        {
            AILog("Production d'un Building généraliste");
            return CreateNewItem(
                UnityEngine.Random.Range(0.1f, 0.2f),
                UnityEngine.Random.Range(0.1f, 0.2f),
                UnityEngine.Random.Range(1, 5),
                UnityEngine.Random.Range(0.1f, 0.5f),
                0,
                10
            );
        }

        // si on arrive ici c'est que l'IA est en retard dans aucun domaine
        if(UnityEngine.Random.Range(0f, 1f) <= advantageFactor)
        {
            AILog("Production d'un Building généraliste");
            return CreateNewItem(
                UnityEngine.Random.Range(0.05f, 0.15f),
                UnityEngine.Random.Range(0.05f, 0.10f),
                UnityEngine.Random.Range(1, 3),
                UnityEngine.Random.Range(0.05f, 0.2f),
                UnityEngine.Random.Range(0.05f, 0.15f),
                10
            );
        }


        return null;
    }

    private BuildingProductionItem CreateNewItem(float food, float gold, int health, float prod, float science, float cost)
    {
        BuildingProductionItem item = ScriptableObject.CreateInstance<BuildingProductionItem>();

        item.bonusFood = food;
        item.bonusGold = gold;
        item.bonusHealth = health;
        item.bonusProduction = prod;
        item.bonusScience = science;
        item.costInProduction = cost;

        return item;
    }

    private void ProcessUnits() // differencier les actives des inactives ; inactives = nouvel ordre ; active = verification de l'objectif et control
    {
        List<HexCell> targetsCells = new List<HexCell>();
        foreach (var cell in grid.cells.Values)
        {
            if (cell.militaryUnit != null && cell.militaryUnit.master == PlayerManager.instance.player)
            {
                targetsCells.Add(cell);
            }
        }
        foreach (var city in CityManager.instance.cities.Values)
        {
            if(city.master == PlayerManager.instance.player)
            {
                targetsCells.Add(city.occupiedCell);
            }
        }

        List<HexCell> controlledCells = new List<HexCell>();
        foreach(var city in cities)
        {
            controlledCells.AddRange(city.controlledTiles.Values);
            controlledCells.Remove(city.occupiedCell);
        }

        foreach (AIUnit AIUnit in units)
        {
            Debug.Log(AIUnit.unit.id + " " + IsUnitInactive(AIUnit.unit));
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
                else
                {
                    if(diplomacy == AI_Diplomacy.Neutral)
                    {
                        if (!controlledCells.Contains(AIUnit.cell) && controlledCells.Count != 0)
                        {
                            HexCell closestControlledCell = null;
                            float shortestPathCost = Mathf.Infinity;

                            foreach(var cell in controlledCells)
                            {
                                if (cell.militaryUnit == null)
                                {
                                    float pathCost = 0;
                                    List<HexCell> path = UnitManager.instance.GetShortestPath(AIUnit.cell, cell, AIUnit.unit.unitType, true);
                                    if (path != null)
                                    {
                                        foreach (var pathCell in path)
                                        {
                                            pathCost += pathCell.terrainType.terrainCost;
                                        }
                                    }
                                    if (pathCost < shortestPathCost)
                                    {
                                        shortestPathCost = pathCost;
                                        closestControlledCell = cell;
                                    }
                                }
                            }

                            if(closestControlledCell != null)
                            {
                                UnitManager.instance.QueueUnitMovement(AIUnit.cell, closestControlledCell, UnitType.UnitCategory.military, delegate { }, true);
                                controlledCells.Remove(closestControlledCell);

                                AILog("warrior started moving to "+closestControlledCell.offsetCoordinates);
                            }
                        }
                    }
                    else
                    {
                        if(targetsCells.Count == 0)
                        {
                            Debug.Log($"<color=#FF0000><b>AI WON</b></color>");
                            aiWonText.gameObject.SetActive(true);
                        }
                        else
                        {
                            AILog("attacking from " + AIUnit.cell.offsetCoordinates + " to " + targetsCells[0].offsetCoordinates);
                            UnitManager.instance.QueueUnitMovement(AIUnit.cell, targetsCells[0], UnitType.UnitCategory.military, delegate { }, true);
                            targetsCells.RemoveAt(0);
                        }
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

    public bool IsUnitInactive(Unit unit)
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

                HexCell cell = grid.GetTile(coord);

                if (CityManager.instance.tileToCity.ContainsKey(coord))
                    continue;

                if (cell == null)
                    continue;

                if (!cell.terrainType.build.Contains(Building.BuildingNames.City))
                    continue;

                if(cell.militaryUnit!=null || cell.civilianUnit!=null)
                    continue;

                if (UnitManager.instance.GetShortestPath(position, cell, UnitManager.instance.GetUnitType("Settler")) == null)
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
        AIUnit AI_unit = GetAIUnit(unit.id);
        if(AI_unit != null)
        {
            units.Remove(AI_unit);
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

    public void ProcessDiplomacy()
    {
        foreach(City city in cities)
        {
            foreach(HexCell cell in city.controlledTiles.Values)
            {
                if ((cell.militaryUnit != null && cell.militaryUnit.master != AI_Player) || (cell.civilianUnit != null && cell.civilianUnit.master != AI_Player))
                {
                    AILog("diplomacy became offensive");
                    diplomacy = AI_Diplomacy.Offensive;
                    turnsSinceOffensive = 0;
                }
            }
        }

        foreach(var AI_unit in units)
        {
            if (AI_unit.unit.lastDamagingTurn >= TurnManager.instance.currentTurn-1)
            {
                AILog("diplomacy became offensive");
                diplomacy = AI_Diplomacy.Offensive;
                turnsSinceOffensive = 0;
            }
        }

        if(diplomacy == AI_Diplomacy.Offensive)
        {
            turnsSinceOffensive++;
        }
        if(turnsSinceOffensive >= nonAgressionTurnsNumber)
        {
            AILog("diplomacy became neutral");
            diplomacy = AI_Diplomacy.Neutral;
        }
    }

    public  AIUnit GetAIUnit(int unitId)
    {
        foreach(var AI_unit in units)
        {
            if (AI_unit.unit.id == unitId)
            {
                return AI_unit;
            }
        }

        return null;
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
