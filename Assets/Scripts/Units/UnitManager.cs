using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    const float UNITS_TURN_SPEED = 0.5f;
    public string[] NAMES_LIST = { "Abel", "Achille", "Adam", "Adolphe", "Adrien", "Aimable", "Aimé", "Alain", "Alan", "Alban", "Albert", "Albin", "Alex", "Alexandre", "Alexis", "Alfred", "Aliaume", "Alix", "Aloïs", "Alphonse", "Amaury", "Ambroise", "Amédée", "Amour", "Ananie", "Anastase", "Anatole", "André", "Andréa", "Ange", "Anicet", "Anselme", "Antelme", "Anthelme", "Anthony", "Antoine", "Antonin", "Apollinaire", "Ariel", "Aristide", "Armand", "Armel", "Arnaud", "Arsène", "Arthur", "Aubin", "Auguste", "Augustin", "Aurélien", "Axel", "Aymard", "Aymeric", "Balthazar", "Baptiste", "Baptistin", "Barnabé", "Barnard", "Barthélémy", "Basile", "Bastien", "Baudouin", "Benjamin", "Benoît", "Bérenger", "Bernard", "Bernardin", "Bertrand", "Bienvenu", "Blaise", "Boris", "Briac", "Brice", "Bruno", "Calixte", "Camille", "Casimir", "Cédric", "Céleste", "Célestin", "César", "Charles", "Charlie", "Christian", "Christophe", "Claude", "Clément", "Clovis", "Colin", "Côme", "Constant", "Constantin", "Corentin", "Crépin", "Cyprien", "Cyril", "Cyrille", "Damien", "Daniel", "Dany", "David", "Davy", "Denis", "Désiré", "Didier", "Dimitri", "Dominique", "Donald", "Donatien", "Dorian", "Eden", "Edgar", "Edgard", "Edmond", "Edouard", "Elias", "Elie", "Eloi", "Emile", "Emilien", "Emmanuel", "Eric", "Ernest", "Erwan", "Erwann", "Etienne", "Eudes", "Eugène", "Evrard", "abien", "Fabrice", "Faustin", "Félicien", "Félix", "Ferdinand", "Fernand", "Fiacre", "Fidèle", "Firmin", "Flavien", "Florent", "Florentin", "Florian", "Floribert", "Fortuné", "Francis", "Franck", "François", "Frédéric", "Fulbert", "Gabin", "Gabriel", "Gaël", "Gaétan", "Gaëtan", "Gaspard", "Gaston", "Gatien", "Gauthier", "Gautier", "Geoffroy", "Georges", "Gérald", "Gérard", "Géraud", "Germain", "Gervais", "Ghislain", "Gilbert", "Gildas", "Gilles", "Godefroy", "Goeffrey", "Gontran", "Gonzague", "Gratien", "Grégoire", "Gregory", "Guénolé", "Guilain", "Guilem", "Guillaume", "Gustave", "Guy", "Guylain", "Gwenaël", "Gwendal", "Habib", "Hadrien", "Hector", "Henri", "Herbert", "Hercule", "Hermann", "Hervé", "Hippolythe", "Honoré", "Honorin", "Horace", "Hubert", "Hugo", "Hugues", "Hyacinthe", "Ignace", "Igor", "Isidore", "Ismaël", "Jacky", "Jacob", "Jacques", "Jean", "Jérémie", "Jérémy", "Jérôme", "Joachim", "Jocelyn", "Joël", "Johan", "Jonas", "Jonathan", "Jordan", "José", "Joseph", "Joshua", "Josselin", "Josué", "Judicaël", "Jules", "Julian", "Julien", "Juste", "Justin", "Kévin", "Lambert", "Lancelot", "Landry", "Laurent", "Lazare", "Léandre", "Léger", "Léo", "Léon", "Léonard", "Léonce", "Léopold", "Lilian", "Lionel", "Loan", "Loïc", "Loïck", "Loris", "Louis", "Louison", "Loup", "Luc", "Luca", "Lucas", "Lucien", "Ludovic", "Maël", "Mahé", "Maixent", "Malo", "Manuel", "Marc", "Marceau", "Marcel", "Marcelin", "Marcellin", "Marin", "Marius", "Martial", "Martin", "Martinien", "Matéo", "Mathéo", "Mathias", "Mathieu", "Mathis", "Mathurin", "Mathys", "Mattéo", "Matthias", "Matthieu", "Maurice", "Maxence", "Maxime", "Maximilien", "Médard", "Melchior", "Merlin", "Michael", "&", "dérivés", "Michel", "Milo", "Modeste", "Morgan", "Naël", "Narcisse", "Nathan", "Nathanaël", "Nestor", "Nicolas", "Noa", "Noah", "Noé", "Noël", "Norbert", "Octave", "Octavien", "Odilon", "Olivier", "Omer", "Oscar", "Pacôme", "Parfait", "Pascal", "Patrice", "Patrick", "Paul", "Paulin", "Perceval", "Philémon", "Philibert", "Philippe", "Pierre", "Pierrick", "Prosper", "Quentin", "Rafaël", "Raoul", "Raphaël", "Raymond", "Réginald", "Régis", "Rémi", "Rémy", "Renaud", "René", "Reynald", "Richard", "Robert", "Robin", "Rodolphe", "Rodrigue", "Roger", "Roland", "Romain", "Romaric", "Roméo", "Romuald", "Ronan", "Sacha", "Salomon", "Sam", "Sami", "Samson", "Samuel", "Samy", "Sasha", "Saturnin", "Sébastien", "Séraphin", "Serge", "Séverin", "Sidoine", "Siméon", "Simon", "Sixte", "Stanislas", "Stéphane", "Sylvain", "Sylvère", "Sylvestre", "Tancrède", "Tanguy", "Théo", "Théodore", "Théophane", "Théophile", "Thibaud", "Thibaut", "Thierry", "Thilbault", "Thomas", "Tibère", "Timéo", "Timothé", "Timothée", "Titouan", "Tristan", "Tyméo", "Ulrich", "Ulysse", "Urbain", "Uriel", "Valentin", "Valère", "Valérien", "Valéry", "Valmont", "Venceslas", "Vianney", "Victor", "Victorien", "Vincent", "Virgile", "Vivien", "Wilfrid", "William", "Xavier", "Yaël", "Yanis", "Yann", "Yannick", "Yohan", "Yves", "Yvon", "Yvonnick", "Zacharie", "Zéphirin" };


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
                    unit.movesDone = 0;
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

                    Vector3 currentCellDir = currentCell.tile.position-unit.unitTransform.position;
                    currentCellDir.y = 0;
                    Quaternion rot = Quaternion.LookRotation(currentCellDir);
                    if (rot != Quaternion.identity)
                    {
                        for (float t = 0; t < 1; t += Time.deltaTime / UNITS_TURN_SPEED)
                        {
                            unit.unitTransform.rotation = Quaternion.Slerp(unit.unitTransform.rotation, rot, t);
                            yield return null;
                        }
                    }
                    unit.unitTransform.rotation = rot;

                    Vector3 currentCellPos = new Vector3(currentCell.tile.position.x, currentCell.terrainHigh, currentCell.tile.position.z);
                    for (float t = 0; t < 1; t += Time.deltaTime / UNITS_SPEED)
                    {
                        unit.unitTransform.position = Vector3.Lerp(unit.unitTransform.position, currentCellPos, t);
                        yield return null;
                    }
                    unit.unitTransform.transform.position = currentCellPos;
                }
                else
                {
                    yield break;
                }

                lastCell = currentCell;
                grid.RevealTilesInRadius(currentCell.offsetCoordinates, unit.unitType.tileRevealRadius, false);
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

                    Vector3 currentCellDir = currentCell.tile.position;
                    currentCellDir.y = unit.unitTransform.position.y;
                    Quaternion rot = Quaternion.LookRotation(currentCellDir);
                    for (float t=0; t<1; t+=Time.deltaTime / UNITS_TURN_SPEED)
                    {
                        unit.unitTransform.rotation = Quaternion.Slerp(unit.unitTransform.rotation, rot, t);
                        yield return null;
                    }

                    Vector3 currentCellPos = new Vector3(currentCell.tile.position.x, currentCell.terrainHigh, currentCell.tile.position.z);
                    for (float t = 0; t < 1; t += Time.deltaTime / UNITS_SPEED)
                    {
                        unit.unitTransform.position = Vector3.Lerp(unit.unitTransform.position, currentCellPos, t);
                        yield return null;
                    }

                    unit.unitTransform.position = currentCellPos;
                }
                else
                {
                    yield break;
                }

                lastCell = currentCell;
                grid.RevealTilesInRadius(currentCell.offsetCoordinates, unit.unitType.tileRevealRadius, false);
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
        Debug.Log(movementData.unitToAttackId + " " + GetEuclideanDistance(path[0], path[path.Count - 1]) +" "+ unit.movesDone);
        while (path.Count>1 && unit.movesDone + pathCost + path[1].terrainType.terrainCost <= unit.unitType.MoveReach && (movementData.unitToAttackId==-1 || GetEuclideanDistance(path[0], path[path.Count-1]) > unit.GetUnitMilitaryData().AttackRange))
        {
            nextMoves.Enqueue(path[1]);
            pathCost += path[1].terrainType.terrainCost;
            path.RemoveAt(0);
        }

        PrintList(path);

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

    public bool QueueUnitMovement(HexCell unitCell, HexCell destCell, UnitType.UnitCategory unitCategory)
    {
        queuedMovementData movementData = new queuedMovementData();
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

        List<HexCell> path = GetShortestPath(unitCell, destCell, unit.unitType, 1f);
        if (path.Count <= 1)
        {
            return false;
        }
        movementData.path = path;

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
            return true;
        }
        return false;
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

        units.Remove(unit.id);
        Destroy(unit.unitPin.gameObject);
        Destroy(unit.unitTransform.gameObject);
    }

    public void AddUnit(UnitType unitType, HexCell cell, Player master)
    {
        if(unitType.unitCategory == UnitType.UnitCategory.military)
        {
            if(cell.militaryUnit == null && isCellTraversable(cell.terrainType, unitType))
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
            if (cell.militaryUnit == null && isCellTraversable(cell.terrainType, unitType))
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

    private List<HexCell> GetShortestPath(HexCell startCell, HexCell finishCell, UnitType unitType, float heuristicFactor)
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
                else if (isCellTraversable(currentCellData.cell.neighbours[i].terrainType, unitType))
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
                            cellsToVisit.RemoveAt(isCellDataInToVisit);
                            AddNewCellData(currentCellNeighboursData, cellsToVisit);
                        }
                    }
                    else if (currentCellNeighboursData.FCost < visitedCells[isCellDataVisited].FCost)
                    {
                        visitedCells.RemoveAt(isCellDataVisited);
                        AddNewCellData(currentCellNeighboursData, visitedCells);
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

    public float GetEuclideanDistance(HexCell cell1, HexCell cell2)
    {
        float euclideanDistance = Vector3.Distance(cell1.cubeCoordinates, cell2.cubeCoordinates)/grid.hexSize;
        float realDistance = euclideanDistance/Mathf.Sqrt(2);
        return Mathf.Round(realDistance * 100) / 100;
    }

    private CellData CreateCellData(CellData parentCellData, HexCell cell, HexCell destCell, float heuristicFactor)
    {
        float GCost = GetEuclideanDistance(cell, destCell);
        float HCost = cell.terrainType.terrainCost * HEURISTIC_SCALING;
        float FCost = GCost + HCost * heuristicFactor;
        return new CellData(GCost, HCost, FCost, cell, parentCellData);
    }

    private bool isCellTraversable(TerrainType terrainType, UnitType unitType)
    {
        if(terrainType.terrainCost > unitType.MoveReach)
        {
            return false;
        }
        if((terrainType.isWater && !unitType.IsABoat) || (!terrainType.isWater && unitType.IsABoat))
        {
            return false;
        }

        return true;
    }

    private void PrintList(List<HexCell> list) // DEBUGAGE; A VIRER
    {
        string BLABLA = "";
        foreach (HexCell cell in list)
        {
            BLABLA += cell.offsetCoordinates.ToString() + " ";
        }
        Debug.Log(BLABLA);
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
    public readonly int id;
    public Transform unitTransform;
    public readonly UnitType unitType;
    public readonly Player master;
    public readonly UnitPin unitPin;
    public string unitName;

    private MilitaryUnitType militaryUnitType;
    private float currentHealth;

    public float movesDone;
    public int lastDamagingTurn { get; private set; }

    public Unit(Transform unitTransform, UnitType unitType, UnitPin unitPin, Player master)
    {
        this.id = UnitManager.instance.nextAvailableId;
        UnitManager.instance.nextAvailableId++;

        this.unitTransform = unitTransform;
        this.unitType = unitType;
        this.unitPin = unitPin;
        this.master = master;

        this.movesDone = 0;
        this.lastDamagingTurn = -1;
        this.unitName = UnitManager.instance.NAMES_LIST[Random.Range(0, UnitManager.instance.NAMES_LIST.Length - 1)];

        unitPin.InitializePin(this.unitType.unitIcon, this.master.livery);
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