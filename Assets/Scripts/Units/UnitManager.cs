using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class UnitManager : MonoBehaviour
{
    [SerializeField] HexGrid grid;
    [SerializeField] GameObject unitPinPrefab;
    [SerializeField] Transform unitPinCanvas;
    [SerializeField] Transform unitContainer;

    [Header("Units")]
    public List<MilitaryUnitType> militaryUnits = new List<MilitaryUnitType>();
    public List<CivilianUnitType> civilianUnits = new List<CivilianUnitType>();

    [HideInInspector] public int nextAvailableId = 0;
    private Dictionary<int, Unit> units = new Dictionary<int, Unit>();
    private Dictionary<int, queuedMovementData> queuedUnitMovements = new Dictionary<int, queuedMovementData>();

    const float HEURISTIC_SCALING = 1.5f;
    const int MAX_ITERATIONS = 10000;
    const float UNITS_SPEED = 0.2f;

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

    public void Start()
    {
        TurnManager.instance.OnTurnChange += UpdateUnits;
    }

    public void UpdateUnits()
    {
        foreach (var unit in units.Values)
        {
            unit.movesDone = 0;

            if (unit.unitType.unitCategory == UnitType.UnitCategory.military)
            {
                if (unit.lastDamagingTurn < TurnManager.instance.currentTurn - 1)
                {
                    unit.Heal(unit.GetUnitMilitaryData().HealthRegeneration);
                }
            }
        }

        List<int> finishedMovements = new List<int>();
        foreach (var unitId in queuedUnitMovements.Keys)
        {
            if (MoveQueuedUnit(unitId))
            {
                finishedMovements.Add(unitId);
            }
        }

        foreach (var unitId in finishedMovements)
        {
            queuedUnitMovements.Remove(unitId);
        }
    }

    public IEnumerator MoveUnit(Queue<HexCell> nextMoves, Unit unit, HexCell cellToAttack)
    {
        HexCell lastCell = nextMoves.Dequeue();
        if (unit.unitType.unitCategory == UnitType.UnitCategory.military)
        {
            HexCell currentCell = lastCell;
            while(nextMoves.Count > 0)
            {
                currentCell = nextMoves.Dequeue();

                if(currentCell.militaryUnit == null)
                {
                    currentCell.militaryUnit = unit;
                    lastCell.militaryUnit = null;

                    Vector3 currentCellPos = new Vector3(currentCell.tile.position.x, currentCell.terrainHigh, currentCell.tile.position.z);
                    Vector3 unitStep = (currentCellPos-unit.unitTransform.position)*Time.smoothDeltaTime/UNITS_SPEED;

                    for (float i=0; i<UNITS_SPEED; i+=Time.deltaTime)
                    {
                        unit.unitTransform.position += unitStep;
                        yield return null;
                    }
                    unit.unitTransform.position = currentCellPos;
                }
                else
                {
                    yield break;
                }

                lastCell = currentCell;
            }

            if (cellToAttack != null)
            {
                UnitFight(currentCell, cellToAttack);
            }
        }
        else
        {
            HexCell currentCell = null;
            while (nextMoves.Count > 0)
            {
                currentCell = nextMoves.Dequeue();

                if (currentCell.civilianUnit == null)
                {
                    currentCell.civilianUnit = unit;
                    lastCell.civilianUnit = null;

                    Vector3 currentCellPos = new Vector3(currentCell.tile.position.x, currentCell.terrainHigh, currentCell.tile.position.z);
                    Vector3 unitStep = (currentCellPos - lastCell.tile.position) * Time.smoothDeltaTime;

                    for (float i = 0; i < UNITS_SPEED; i += Time.deltaTime)
                    {
                        unit.unitTransform.position += unitStep;
                        yield return null;
                    }
                    unit.unitTransform.position = currentCellPos;
                }
                else
                {
                    yield break;
                }

                lastCell = currentCell;
            }
        }
    }

    public bool MoveQueuedUnit(int unitId)
    {
        queuedMovementData movementData = queuedUnitMovements[unitId];
        List<HexCell> path = movementData.path;
        Unit unit = units[unitId];

        Queue<HexCell> nextMoves = new Queue<HexCell>();
        nextMoves.Enqueue(path[0]);

        if (movementData.unitToAttackId!=-1 && (path[path.Count - 1].militaryUnit == null || path[path.Count-1].militaryUnit.id != movementData.unitToAttackId))
        {
            return true;
        }

        float pathCost = 0f;
        while (path.Count>1 && unit.movesDone + pathCost + path[1].terrainType.terrainCost <= unit.unitType.MoveReach && (movementData.unitToAttackId==-1 || GetEuclideanDistance(path[0], path[path.Count-1]) > unit.GetUnitMilitaryData().AttackRange))
        {
            nextMoves.Enqueue(path[1]);
            pathCost += path[1].terrainType.terrainCost;
            path.RemoveAt(0);
        }

        if (movementData.unitToAttackId != -1 && GetEuclideanDistance(path[0], path[path.Count - 1]) <= unit.GetUnitMilitaryData().AttackRange)
        {
            StartCoroutine(MoveUnit(nextMoves, unit, path[path.Count-1]));
        }
        else
        {
            StartCoroutine(MoveUnit(nextMoves, unit, null));
        }
        unit.movesDone += pathCost;

        if (path.Count <= 1)
        {
            return true;
        }
        return false;
    }

    public void QueueUnitMovement(HexCell unitCell, HexCell destCell, UnitType.UnitCategory unitCategory)
    {
        List<HexCell> path = GetShortestPath(unitCell, destCell, 1f);

        if (path.Count <= 1)
        {
            return;
        }

        queuedMovementData movementData = new queuedMovementData();
        movementData.path = path;
        movementData.unitToAttackId = -1;

        Unit unit;
        if (unitCategory == UnitType.UnitCategory.military)
        {
            unit = unitCell.militaryUnit;
            if (destCell.militaryUnit != null)
            {
                movementData.unitToAttackId = destCell.militaryUnit.id;
            }
        }
        else
        {
            unit = unitCell.civilianUnit;
        }

        if (queuedUnitMovements.ContainsKey(unit.id))
        {
            queuedUnitMovements[unit.id] = movementData;
        }
        else
        {
            queuedUnitMovements.Add(unit.id, movementData);
        }
            
        if (MoveQueuedUnit(unit.id))
        {
            queuedUnitMovements.Remove(unit.id);
        }
    }

    private void UnitFight(HexCell attackerCell, HexCell defenderCell)
    {
        float cellDistance = GetEuclideanDistance(attackerCell, defenderCell);

        defenderCell.militaryUnit.TakeDamage(attackerCell.militaryUnit.GetUnitMilitaryData().AttackPower);

        if(cellDistance <= defenderCell.militaryUnit.GetUnitMilitaryData().AttackRange)
        {
            attackerCell.militaryUnit.TakeDamage(defenderCell.militaryUnit.GetUnitMilitaryData().DefensePower);
        }

        if (!attackerCell.militaryUnit.IsAlive())
        {
            RemoveUnit(UnitType.UnitCategory.military, attackerCell);
        }
        if (!defenderCell.militaryUnit.IsAlive())
        {
            RemoveUnit(UnitType.UnitCategory.military, defenderCell);
        }
    }

    public void RemoveUnit(UnitType.UnitCategory unitCategory, HexCell unitCell)
    {
        Unit unit;
        if (unitCategory == UnitType.UnitCategory.military)
        {
            unit = unitCell.militaryUnit;
            unitCell.militaryUnit = null;
        }
        else
        {
            unit = unitCell.civilianUnit;
            unitCell.civilianUnit = null;
        }

        Destroy(unit.unitPin.gameObject);
        Destroy(unit.unitTransform.gameObject);
    }

    public void AddUnit(UnitType unitType, HexCell cell, Player master)
    {
        if(unitType.unitCategory == UnitType.UnitCategory.military)
        {
            if(cell.militaryUnit == null)
            {
                Transform unitTransform = Instantiate(
                    unitType.unitPrefab.transform,
                    new Vector3(cell.tile.position.x, cell.terrainHigh, cell.tile.position.z),
                    new Quaternion(0, 0, 0, 1), 
                    unitContainer);
                Transform unitPinTransform = Instantiate(unitPinPrefab.transform, unitPinCanvas);

                UnitPin unitPin = unitPinTransform.GetComponent<UnitPin>();

                Unit unit = new Unit(unitTransform, unitType, unitPin, master);

                cell.militaryUnit = unit;
                units.Add(unit.id, unit);
            }
            else
            {
                Debug.LogError("trying to add a military unit on an already occupied tile");
            }
        }
        else
        {
            if (cell.militaryUnit == null)
            {
                Transform unitTransform = Instantiate(
                    unitType.unitPrefab.transform,
                    new Vector3(cell.tile.position.x, cell.terrainHigh, cell.tile.position.z),
                    new Quaternion(0, 0, 0, 1),
                    unitContainer);
                Transform unitPinTransform = Instantiate(unitPinPrefab.transform, unitPinCanvas);

                UnitPin unitPin = unitPinTransform.GetComponent<UnitPin>();

                Unit unit = new Unit(unitTransform, unitType, unitPin, master);

                cell.civilianUnit = unit;
                units.Add(unit.id, unit);
            }
            else
            {
                Debug.LogError("trying to add a support unit on an already occupied tile");
            }
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
        

        if (endCellFound)
        {
            currentCellData = CreateCellData(currentCellData, finishCell, finishCell, 0f);
            while (currentCellData.cell.offsetCoordinates != startCell.offsetCoordinates)
            {
                pathCoordinates.Insert(0, currentCellData.cell);
                currentCellData = currentCellData.parentCellData;
            }
            pathCoordinates.Insert(0, startCell);

            System.DateTime endTime = System.DateTime.Now;
            Debug.Log("FOUND PATH FROM " + startCell.offsetCoordinates + " TO " + finishCell.offsetCoordinates + " IN " + endTime.Subtract(startTime) + "s AND " + iterations + " iterations");
            PrintList(pathCoordinates);

            return pathCoordinates;
        }
        else
        {
            System.DateTime endTime = System.DateTime.Now;
            Debug.Log("NO PATH FOUND FROM " + startCell.offsetCoordinates + " TO " + finishCell.offsetCoordinates + " IN " + endTime.Subtract(startTime) + "s AND " + iterations + " iterations");
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
        float euclideanDistance = Vector2.Distance(cell1.offsetCoordinates, cell2.offsetCoordinates)/grid.hexSize;
        return Mathf.Round(euclideanDistance * 100) / 100;
    }

    private CellData CreateCellData(CellData parentCellData, HexCell cell, HexCell destCell, float heuristicFactor)
    {
        float GCost = GetEuclideanDistance(cell, destCell);
        float HCost = cell.terrainType.terrainCost * HEURISTIC_SCALING;
        float FCost = GCost + HCost * heuristicFactor;
        return new CellData(GCost, HCost, FCost, cell, parentCellData);
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
    } // DEBUGAGE; A VIRER

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

    public Unit(Transform unitTransform, UnitType unitType, UnitPin unitPin, Player master)
    {
        this.id = UnitManager.instance.nextAvailableId;
        UnitManager.instance.nextAvailableId++;

        this.unitTransform = unitTransform;
        this.unitType = unitType;
        this.unitPin = unitPin;
        this.master = master;

        unitPin.InitializePin(this.unitType.unitSprite, this.master.livery);
        unitPin.worldTarget = this.unitTransform;

        //Debug.Log("NEW UNIT, ID : "+this.id);

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

/*private void Start()
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
    }*/