using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitManager : MonoBehaviour
{
    [SerializeField] HexGrid grid;

    const float HEURISTIC_SCALING = 1.5f;
    const int MAX_ITERATIONS = 10000;

    [Header("Units")]
    [SerializeField] public List<MilitaryUnitType> militaryUnits = new List<MilitaryUnitType>();
    [SerializeField] public List<SupportUnitType> supportUnits = new List<SupportUnitType>();

    public int nextAvailableId = 0;
    public List<Unit> units = new List<Unit>();
    private Dictionary<int, List<HexCell>> queuedUnitMovements = new Dictionary<int, List<HexCell>>();
    private Dictionary<int, List<HexCell>> queuedUnitFights = new Dictionary<int, List<HexCell>>();

    public static UnitManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de UnitManager dans la scene");
            return;
        }
        instance = this;
    }

    private void Start()
    {
        TurnManager.instance.OnTurnChange += MovequeuedUnitMovements;
        TurnManager.instance.OnTurnChange += ResetUnitsMoveCount;
    }

    public void QueueUnitFight(HexCell unitCell, HexCell cellToAttack)
    {
        float tileDistance = GetEuclideanDistance(unitCell, cellToAttack, grid.hexSize);
        if (tileDistance <= unitCell.militaryUnit.militaryUnitType.AttackRange)
        {
            UnitFight(unitCell.militaryUnit, cellToAttack.militaryUnit, tileDistance);
        }

    }

    private void UnitFight(Unit attacker, Unit defender, float tileDistance)
    {
        defender.currentHealth -= attacker.militaryUnitType.AttackPower;
        if (attacker.militaryUnitType.AttackRange>=tileDistance)
        {
            attacker.currentHealth -= defender.militaryUnitType.DefensePower;
        }
    }

    private Unit GetUnitById(int id)
    {
        foreach (var unit in units)
        {
            if (unit.id == id)
            {
                return unit;
            }
        }
        return null;
    }

    private void PrintList(List<HexCell> list)
    {
        string BLABLA = "";
        foreach (HexCell cell in list)
        {
            BLABLA += cell.offsetCoordinates.ToString() + " ";
            //Destroy(cell.tile.gameObject);
        }
        Debug.Log(BLABLA);
    }

    public void HealUnits()
    {
        foreach(var unit in units)
        {
            if (unit.unitType.unitCategory == UnitType.UnitCategory.military)
            {
                unit.currentHealth += unit.militaryUnitType.HealthRegeneration;
            }
        }
    }

    public void ResetUnitsMoveCount()
    {
        foreach(var unit in units)
        {
            unit.movesDone = 0;
        }
    }

    public void MovequeuedUnitMovements()
    {
        List<int> emptyMovementKeys = new List<int>();

        foreach (var unitId in queuedUnitMovements.Keys)
        {
            if (MoveQueuedUnit(unitId))
            {
                emptyMovementKeys.Add(unitId);
            }
        }

        foreach (var unit in emptyMovementKeys)
        {
            queuedUnitMovements.Remove(unit);
        }
    }

    public bool MoveQueuedUnit(int unitId)
    {
        Unit unit = GetUnitById(unitId);
        List<HexCell> path = queuedUnitMovements[unitId];
        HexCell startCell = path[0];
        Queue<HexCell> destCells= new Queue<HexCell>();
        float pathCost = 0f;
        
        while (path.Count > 1 && path[1].terrainType.terrainCost + pathCost <= unit.unitType.MoveReach)
        {
            pathCost += path[1].terrainType.terrainCost;
            destCells.Enqueue(path[1]);
            path.RemoveAt(0);
            PrintList(path);
        }
        unit.movesDone += pathCost;
        StartCoroutine(MoveUnit(unit, startCell, destCells, 1f));
        return path.Count <= 1;
    }

    public void QueueUnitMovement(Unit unit, HexCell unitCell, HexCell destinationCell)
    {
        Debug.Log(unitCell.offsetCoordinates + " " + destinationCell.offsetCoordinates);
        List<HexCell> path = GetShortestPath(unitCell, destinationCell, 1f);

        if (path == null)
        {
            Debug.Log("pas de chemin trouvé");
            return;
        }
        else
        {
            queuedUnitMovements.Add(unit.id, path);
            MoveQueuedUnit(unit.id);
        }
    }

    IEnumerator MoveUnit(Unit unit, HexCell startCell, Queue<HexCell> destCells, float time)
    {
        HexCell lastCell = startCell;
        HexCell destCell;
        while (destCells.Count > 0)
        {
            
            destCell = destCells.Dequeue();
            if (unit.unitType.unitCategory == UnitType.UnitCategory.military)
            {
                if (destCell.militaryUnit == null)
                {
                    lastCell.militaryUnit = null;
                    for (float t = 0; t < 1; t += Time.deltaTime / time)
                    {
                        Vector3 newPos = new Vector3(destCell.tile.position.x, destCell.terrainHigh, destCell.tile.position.z);
                        unit.unitTransform.position = Vector3.Lerp(unit.unitTransform.position, newPos, t);
                        yield return null;
                    }
                }
                else
                {
                    // A REPENSER
                    if (unit.master != destCell.militaryUnit.master)
                    {
                        //UnitFight(unit, destCell.militaryUnit);
                    }
                }
            }
            else
            {
                if (destCell.supportUnit == null)
                {
                    lastCell.supportUnit = null;
                    for (float t = 0; t < 1; t += Time.deltaTime / time)
                    {
                        unit.unitTransform.position = Vector3.Lerp(unit.unitTransform.position, destCell.tile.position, t);
                        yield return null;
                    }
                }
                else
                {
                    Debug.LogWarning("trying to add a unit on an alreday occupied tile");
                }
            }
            lastCell = destCell;
            grid.RevealTilesInRadius(unit.unitTransform.position, 2);
        }
    }

    public Unit AddUnit(HexCell cell, UnitType unitType)
    {
        if (!cell.isActive || !cell.isRevealed)
        {
            Debug.LogWarning("trying to add a unit on a not active tile");
            //return;
        }
        if (unitType.unitCategory == UnitType.UnitCategory.military)
        {
            if (cell.militaryUnit == null)
            {
                Transform unitTransform = Instantiate(
                    unitType.Prefab,
                    new Vector3(cell.tile.position.x, cell.terrainHigh, cell.tile.position.z),
                    new Quaternion(0, 0, 0, 1),
                    this.transform
                    );

                Unit unit = new Unit(unitTransform, unitType, "tamer");
                units.Add(unit);

                cell.militaryUnit = unit;
                return unit;
            }
            else
            {
                Debug.LogWarning("trying to add a unit on an alreday occupied tile");
            }
        }
        else
        {
            if (cell.supportUnit == null)
            {
                Transform unitTransform = Instantiate(
                    unitType.Prefab,
                    new Vector3(cell.tile.position.x, cell.terrainHigh, cell.tile.position.z),
                    new Quaternion(0, 0, 0, 1),
                    this.transform
                    );

                Unit unit = new Unit(unitTransform, unitType, "tamer");
                units.Add(unit);

                cell.supportUnit = unit;
                return unit;
            }
            else
            {
                Debug.LogWarning("trying to add a unit on an alreday occupied tile");
            }
        }
        return null;
    }

    public void RemoveUnit(HexCell cell, UnitType.UnitCategory unitCategory)
    {
        if (unitCategory == UnitType.UnitCategory.military)
        {
            Destroy(cell.militaryUnit.unitTransform.gameObject, 1f);
            cell.militaryUnit = null;
        }
        else
        {
            Destroy(cell.militaryUnit.unitTransform.gameObject);
            cell.supportUnit = null;
        }
    }

    public List<HexCell> GetShortestPath(HexCell startCell, HexCell finishCell, float heuristicFactor)
    {
        //Debug.LogWarning("SEARCHING PATH FROM "+startCell.offsetCoordinates + " TO "+finishCell.offsetCoordinates);

        bool endCellFound = false;
        List<HexCell> pathCoordinates = new List<HexCell>();
        CellData startCellData = CreateCellData(null, startCell, finishCell, heuristicFactor, grid.hexSize);

        List<CellData> visitedCells = new List<CellData>();
        List<CellData> cellsToVisit = new List<CellData>() { startCellData };

        CellData currentCellData = null;

        int iterations = 0;
        System.DateTime startTime = System.DateTime.Now;
        while (!endCellFound && cellsToVisit.Count > 0 && iterations < MAX_ITERATIONS)
        {
            currentCellData = cellsToVisit[0];
            AddNewCellData(currentCellData, visitedCells);
            cellsToVisit.RemoveAt(0);
            //Debug.LogWarning("looking at cell : " + currentCellData.GetCellDataInfo());

            for (int i = 0; i < 6; i++)
            {
                if (currentCellData.cell.neighbours[i]!=null && currentCellData.cell.neighbours[i].offsetCoordinates == finishCell.offsetCoordinates)
                {
                    endCellFound = true;
                    break;
                }
                else if (currentCellData.cell.neighbours[i] != null && currentCellData.cell.neighbours[i].terrainType.traversable)
                {
                    CellData currentCellNeighboursData = CreateCellData(currentCellData, currentCellData.cell.neighbours[i], finishCell, heuristicFactor, grid.hexSize);
                    //Debug.Log("looking at neighbour cell : " + currentCellNeighboursData.GetCellDataInfo());
                    int isCellDataVisited = GetCellDataIndex(currentCellNeighboursData, visitedCells);
                    if (isCellDataVisited == -1)
                    {
                        int isCellDataInToVisit = GetCellDataIndex(currentCellNeighboursData, cellsToVisit);
                        if (isCellDataInToVisit == -1)
                        {
                            AddNewCellData(currentCellNeighboursData, cellsToVisit);
                        }
                        else if (currentCellNeighboursData.FCost < cellsToVisit[isCellDataInToVisit].FCost)
                        {
                            cellsToVisit[isCellDataInToVisit] = currentCellNeighboursData;
                        }
                    }
                    else if (currentCellNeighboursData.FCost < visitedCells[isCellDataVisited].FCost)
                    {
                        visitedCells[isCellDataVisited] = currentCellNeighboursData;
                    }
                }
            }

            iterations++;
        }
        System.DateTime endTime = System.DateTime.Now;

        Debug.Log("iterations : " + iterations);
        //Debug.Log("time taken : " + endTime.Subtract(startTime));
        Debug.Log("SEARCHING PATH FROM " + startCell.offsetCoordinates + " TO " + finishCell.offsetCoordinates + " IN " + endTime.Subtract(startTime));

        if (endCellFound)
        {
            currentCellData = CreateCellData(currentCellData, finishCell, finishCell, 0f, grid.hexSize);
            while (currentCellData.cell.offsetCoordinates != startCell.offsetCoordinates)
            {
                pathCoordinates.Add(currentCellData.cell);
                currentCellData = currentCellData.parentCellData;
            }
            pathCoordinates.Reverse();
            pathCoordinates.Insert(0, startCell);

            return pathCoordinates;
        }
        else
        {
            return null;
        }
    }

    private void AddNewCellData(CellData cellData, List<CellData> cellDataList)
    {
        int i = 0;
        while (i < cellDataList.Count && cellDataList[i].FCost < cellData.FCost)
        {
            i++;
        }

        if (i == cellDataList.Count)
        {
            cellDataList.Add(cellData);
            return;
        }
        else if (cellData.FCost == cellDataList[i].FCost)
        {
            while (i < cellDataList.Count && cellDataList[i].FCost == cellData.FCost && cellDataList[i].HCost < cellData.HCost)
            {
                i++;
            }
        }

        if (i == cellDataList.Count)
        {
            cellDataList.Add(cellData);
            return;
        }
        cellDataList.Insert(i, cellData);
        return;
    }

    private int GetCellDataIndex(CellData cellData, List<CellData> cellDataList)
    {
        for (int i = 0; i < cellDataList.Count; i++)
        {
            if (cellDataList[i].cell.offsetCoordinates == cellData.cell.offsetCoordinates)
            {
                return i;
            }
        }
        return -1;
    }

    private float GetEuclideanDistance(HexCell cell1, HexCell cell2, float hexSize)
    {
        float xDiff = Mathf.Pow(cell1.offsetCoordinates.x - cell2.offsetCoordinates.x, 2);
        float yDiff = Mathf.Pow(cell1.offsetCoordinates.y - cell2.offsetCoordinates.y, 2);
        float euclideanDistance = Mathf.Sqrt(xDiff + yDiff) / hexSize;
        return Mathf.Round(euclideanDistance * 100) / 100;
    }

    private CellData CreateCellData(CellData parentCellData, HexCell cell, HexCell destCell, float heuristicFactor, float hexSize)
    {
        float GCost = GetEuclideanDistance(cell, destCell, hexSize);
        float HCost = cell.terrainType.terrainCost * HEURISTIC_SCALING;
        float FCost = GCost + HCost * heuristicFactor;
        return new CellData(GCost, HCost, FCost, cell, parentCellData);
    }

    private class CellData
    {
        private readonly float GCost;
        public readonly float HCost;
        public readonly float FCost;
        public readonly HexCell cell;
        public readonly CellData parentCellData;

        public CellData(float GCost, float HCost, float FCost, HexCell cell, CellData parentCellData)
        {
            this.GCost = GCost;
            this.HCost = HCost;
            this.FCost = FCost;
            this.cell = cell;
            this.parentCellData = parentCellData;
        }

        public string GetCellDataInfo()
        {
            return this.cell.offsetCoordinates.ToString() + " " + this.GCost.ToString() + " " + HCost.ToString() + " " + this.FCost.ToString();
        }
    }
}

public class Unit
{
    public int id;
    public Transform unitTransform;
    public UnitType unitType;
    public string master;

    public MilitaryUnitType militaryUnitType;
    public float currentHealth;
    public float movesDone;

    public Unit(Transform unitTransform, UnitType unitType, string master)
    {
        this.id = UnitManager.instance.nextAvailableId;
        UnitManager.instance.nextAvailableId++;

        this.unitTransform = unitTransform;
        this.unitType = unitType;
        this.master = master;

        Debug.Log("NEW UNIT, ID : "+this.id);

        if (unitType.unitCategory == UnitType.UnitCategory.military)
        {
            this.militaryUnitType = unitType as MilitaryUnitType;
            this.currentHealth = this.militaryUnitType.MaxHealth;
        }
    }
}