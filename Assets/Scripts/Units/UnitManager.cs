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
    [SerializeField] UnitPin unitPinPrefab;
    [SerializeField] Transform unitPinCanvas;

    const float HEURISTIC_SCALING = 1.5f;
    const int MAX_ITERATIONS = 10000;

    [Header("Units")]
    public List<MilitaryUnitType> militaryUnits = new List<MilitaryUnitType>();
    public List<CivilianUnitType> civilianUnits = new List<CivilianUnitType>();

    [HideInInspector] public int nextAvailableId = 0;
    private List<Unit> units = new List<Unit>();
    private Dictionary<int, queuedMovementData> queuedUnitMovements = new Dictionary<int, queuedMovementData>();

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
        TurnManager.instance.OnTurnChange += ExecuteQueuedUnitMovements;
        TurnManager.instance.OnTurnChange += ResetUnitsMoveCount;
        TurnManager.instance.OnTurnChange += RegenerateUnitsHealth;
    }

    private bool UnitFight(HexCell attackerCell, HexCell defenderCell)
    {
        float tileDistance = GetEuclideanDistance(attackerCell, defenderCell);
        Debug.Log(attackerCell.offsetCoordinates + " " + defenderCell.offsetCoordinates);
        Debug.Log(attackerCell.militaryUnit.id + " " + defenderCell.militaryUnit.id);

        defenderCell.militaryUnit.TakeDamage(attackerCell.militaryUnit.GetUnitMilitaryData().AttackPower);

        if (defenderCell.militaryUnit.GetUnitMilitaryData().AttackRange>=tileDistance)
        {
            attackerCell.militaryUnit.TakeDamage(defenderCell.militaryUnit.GetUnitMilitaryData().DefensePower);
        }
        
        if (!attackerCell.militaryUnit.IsAlive())
        {
            RemoveUnit(attackerCell, UnitType.UnitCategory.military);
        }
        if (!defenderCell.militaryUnit.IsAlive())
        {
            RemoveUnit(defenderCell, UnitType.UnitCategory.military);
            return true;
        }

        return false;
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

    public void RegenerateUnitsHealth()
    {
        foreach(var unit in units)
        {
            if (unit.unitType.unitCategory == UnitType.UnitCategory.military)
            {
                if(unit.lastDamagingTurn < TurnManager.instance.currentTurn - 1)
                {
                    unit.Heal(unit.GetUnitMilitaryData().HealthRegeneration);
                }
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

    public void ExecuteQueuedUnitMovements()
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

    private bool MoveQueuedUnit(int unitId)
    {
        Unit unit = GetUnitById(unitId);
        List<HexCell> path = queuedUnitMovements[unitId].path;
        HexCell startCell = path[0];
        HexCell destCell = path[path.Count - 1];

        Queue<HexCell> destCells= new Queue<HexCell>();
        float pathCost = 0f;
        bool isInRange = false;
        
        if (queuedUnitMovements[unitId].unitToAttackId != -1 && destCell.GetUnit(UnitType.UnitCategory.military) == null) // Arrête le pathfinding si la case est inactive
        {
            return true;
        }
        
        Debug.Log(startCell.offsetCoordinates+" ATKS "+destCell.offsetCoordinates);

        while (path.Count > 1 && path[1].terrainType.terrainCost + pathCost <= unit.unitType.MoveReach && !isInRange)
        {
            float distanceToDest = GetEuclideanDistance(destCell, path[0]);
            Debug.Log("distance to target:" + distanceToDest);
            if (queuedUnitMovements[unitId].unitToAttackId != -1 && destCell.militaryUnit != null && destCell.militaryUnit.id == queuedUnitMovements[unitId].unitToAttackId && distanceToDest <= unit.GetUnitMilitaryData().AttackRange)
            {
                isInRange = true;
            }
            else
            {
                pathCost += path[1].terrainType.terrainCost;
                destCells.Enqueue(path[1]);
                path.RemoveAt(0);
            }
        }
        unit.movesDone += pathCost;
        Debug.Log(destCells.Count);

        StartCoroutine(MoveUnit(unit, destCells, 0.1f, isInRange));
        if (unit.unitType.unitCategory == UnitType.UnitCategory.military)
        {
            startCell.militaryUnit = null;
            path[0].militaryUnit = unit;
        }
        else
        {
            startCell.militaryUnit = null;
            path[0].civilianUnit = unit;
        }

        if (isInRange)
        {
            if(UnitFight(path[0], destCell))
            {
                return true;
            }
        }

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
            queuedMovementData movementData = new queuedMovementData();
            movementData.path = path;

            if (destinationCell.GetUnit(UnitType.UnitCategory.military) != null && unit.master.playerName != destinationCell.GetUnit(UnitType.UnitCategory.military).master.playerName)
            {
                movementData.unitToAttackId = destinationCell.militaryUnit.id;
            }
            else
            {
                movementData.unitToAttackId = -1;
            }

            if (queuedUnitMovements.ContainsKey(unit.id))
            {
                queuedUnitMovements[unit.id] = movementData;
            }
            else
            {
                queuedUnitMovements.Add(unit.id, movementData);
            }
            MoveQueuedUnit(unit.id);
        }
    }

    IEnumerator MoveUnit(Unit unit, Queue<HexCell> destCells, float time, bool isInRange)
    {
        HexCell destCell;

        while (destCells.Count > 0)
        {
            destCell = destCells.Dequeue();
            Vector3 startPos = unit.unitTransform.position;
            if (unit.unitType.unitCategory == UnitType.UnitCategory.military)
            {
                for(float t=0; t<time; t+= Time.deltaTime)
                {
                    Vector3 newPos = new Vector3(destCell.tile.position.x, destCell.terrainHigh, destCell.tile.position.z);
                    unit.unitTransform.position += (newPos-startPos)*Time.deltaTime/time;

                    unit.unitPin.worldTarget=unit.unitTransform;

                    yield return null;
                }
                unit.unitTransform.position = new Vector3(destCell.tile.position.x, destCell.terrainHigh, destCell.tile.position.z);
            }
            else
            {
                for (float t = 0; t < time; t += Time.deltaTime)
                {
                    Vector3 newPos = new Vector3(destCell.tile.position.x, destCell.terrainHigh, destCell.tile.position.z);
                    unit.unitTransform.position += (newPos - startPos) * Time.deltaTime / time;

                    unit.unitPin.worldTarget = unit.unitTransform;

                    yield return null;
                }
                unit.unitTransform.position = new Vector3(destCell.tile.position.x, destCell.terrainHigh, destCell.tile.position.z);
            }
            grid.RevealTilesInRadius(unit.unitTransform.position, 2, true);
        }

        if (isInRange)
        {
            // play attack animation
        }
    }

    public Unit AddUnit(HexCell cell, UnitType unitType, Player master)
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
                Unit unit = new Unit(unitTransform, unitType, master);
                units.Add(unit);

                cell.militaryUnit = unit;

                UnitPin unitPin = Instantiate(unitPinPrefab, unitPinCanvas.transform);
                unitPin.worldTarget = unit.unitTransform;
                unitPin.InitializePin(unitType.unitSprite, master.livery);
                
                unit.unitPin=unitPin;
                unit.TakeDamage(0f);

                return unit;
            }
            else
            {
                Debug.LogWarning("trying to add a unit on an alreday occupied tile");
            }
        }
        else
        {
            if (cell.civilianUnit == null)
            {
                Transform unitTransform = Instantiate(
                    unitType.Prefab,
                    new Vector3(cell.tile.position.x, cell.terrainHigh, cell.tile.position.z),
                    new Quaternion(0, 0, 0, 1),
                    this.transform
                    );

                Unit unit = new Unit(unitTransform, unitType, master);
                units.Add(unit);

                cell.civilianUnit = unit;

                UnitPin unitPin = Instantiate(unitPinPrefab, instance.transform);
                unitPin.worldTarget = unit.unitTransform;
                unit.unitPin=unitPin;

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
            Destroy(cell.militaryUnit.unitPin.gameObject, Time.deltaTime);
            Destroy(cell.militaryUnit.unitTransform.gameObject, Time.deltaTime);
            cell.militaryUnit = null;
        }
        else
        {
            Destroy(cell.militaryUnit.unitPin.gameObject, Time.deltaTime);
            Destroy(cell.militaryUnit.unitTransform.gameObject, Time.deltaTime);
            cell.civilianUnit = null;
        }
    }

    private List<HexCell> GetShortestPath(HexCell startCell, HexCell finishCell, float heuristicFactor)
    {
        //Debug.LogWarning("SEARCHING PATH FROM "+startCell.offsetCoordinates + " TO "+finishCell.offsetCoordinates);

        bool endCellFound = false;
        List<HexCell> pathCoordinates = new List<HexCell>();
        CellData startCellData = CreateCellData(null, startCell, finishCell, heuristicFactor);

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
                if (currentCellData.cell.neighbours[i] == null)
                {
                    continue;
                }
                else if (currentCellData.cell.neighbours[i].offsetCoordinates == finishCell.offsetCoordinates)
                {
                    endCellFound = true;
                    break;
                }
                else if (currentCellData.cell.neighbours[i].terrainType.traversable)
                {
                    CellData currentCellNeighboursData = CreateCellData(currentCellData, currentCellData.cell.neighbours[i], finishCell, heuristicFactor);
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
            currentCellData = CreateCellData(currentCellData, finishCell, finishCell, 0f);
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

    private float GetEuclideanDistance(HexCell cell1, HexCell cell2)
    {
        /*float xDiff = Mathf.Pow(cell1.offsetCoordinates.x - cell2.offsetCoordinates.x, 2);
        float yDiff = Mathf.Pow(cell1.offsetCoordinates.y - cell2.offsetCoordinates.y, 2);
        float euclideanDistance = Mathf.Sqrt(xDiff + yDiff) / grid.hexSize;*/
        float euclideanDistance = Vector2.Distance(cell1.tile.position, cell2.tile.position)/grid.hexSize;
        return Mathf.Round(euclideanDistance * 100) / 100;
    }

    private CellData CreateCellData(CellData parentCellData, HexCell cell, HexCell destCell, float heuristicFactor)
    {
        float GCost = GetEuclideanDistance(cell, destCell);
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
    public Player master;
    public UnitPin unitPin;

    private MilitaryUnitType militaryUnitType;
    private float currentHealth;

    public float movesDone;
    public int lastDamagingTurn = -1;

    public Unit(Transform unitTransform, UnitType unitType, Player master)
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

    public void TakeDamage(float damage)
    {
        this.currentHealth -= damage;
        this.unitPin.UpdateHealth(this.currentHealth, this.militaryUnitType.MaxHealth);
        lastDamagingTurn = TurnManager.instance.currentTurn;
    }

    public void Heal(float healAmount)
    {
        this.currentHealth += healAmount;
        
        if(this.currentHealth >= this.militaryUnitType.MaxHealth)
        {
            this.currentHealth = this.militaryUnitType.MaxHealth;
        }

        this.unitPin.UpdateHealth(this.currentHealth, this.militaryUnitType.MaxHealth);
    }

    public bool IsAlive()
    {
        return this.currentHealth >= 0;
    }

    public MilitaryUnitType GetUnitMilitaryData()
    {
        return this.militaryUnitType;
    }
}

public struct queuedMovementData
{
    public int unitToAttackId;
    public List<HexCell> path;
}