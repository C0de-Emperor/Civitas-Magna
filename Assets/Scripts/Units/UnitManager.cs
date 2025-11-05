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
        TurnManager.instance.OnTurnChange += UpdateUnits; // ajoute UpdateUnits à la liste des fonctions appelées à chaquez début de tour
    }

    // met à jour toutes les unités
    public void UpdateUnits()
    {
        foreach (var unit in units.Values)
        {
            unit.movesDone = 0; // réinitialise les mouvements réalisés

            if (unit.unitType.unitCategory == UnitType.UnitCategory.military)
            {
                if (unit.lastDamagingTurn < TurnManager.instance.currentTurn - 1)
                {
                    unit.Heal(unit.GetUnitMilitaryData().HealthRegeneration); // soigne l'unité si elle n'a pas pris de dégats au tour précédent
                }
            }
        }

        List<int> finishedMovements = new List<int>();
        foreach (var unitId in queuedUnitMovements.Keys) // parcours la liste d'attente de déplacement d'unités
        {
            if (MoveQueuedUnit(unitId)) // déplace l'unité
            {
                finishedMovements.Add(unitId); // si il faut arrêter le mouvement, ajoute l'id de l'unité dans la liste des unités à arrêter de déplacer
            }
        }

        foreach (var unitId in finishedMovements)
        {
            queuedUnitMovements.Remove(unitId); // enlever l'unité de la liste d'attente de déplacements
        }
    }

    // déplacement d'unité
    public IEnumerator MoveUnit(Queue<HexCell> nextMoves, Unit unit, HexCell cellToAttack)
    {
        HexCell lastCell = nextMoves.Dequeue();
        if (unit.unitType.unitCategory == UnitType.UnitCategory.military)
        {
            HexCell currentCell = lastCell;
            while(nextMoves.Count > 0)
            {
                currentCell = nextMoves.Dequeue();

                if(currentCell.militaryUnit == null) // vérifie qu'il ne se déplace pas sur une case déjà occupée
                {
                    currentCell.militaryUnit = unit;
                    lastCell.militaryUnit = null; // enlever l'unité de sa case actuelle

                    Vector3 currentCellDir = currentCell.tile.position-unit.unitTransform.position;
                    currentCellDir.y = 0;
                    Quaternion rot = Quaternion.LookRotation(currentCellDir);
                    if (rot != Quaternion.identity)
                    {
                        for (float t = 0; t < 1; t += Time.deltaTime / UNITS_TURN_SPEED)
                        {
                            unit.unitTransform.rotation = Quaternion.Slerp(unit.unitTransform.rotation, rot, t); // rotation progressive de l'unité dans le sens du déplacement
                            yield return null;
                        }
                    }
                    unit.unitTransform.rotation = rot;

                    Vector3 currentCellPos = new Vector3(currentCell.tile.position.x, currentCell.terrainHigh, currentCell.tile.position.z);
                    for (float t = 0; t < 1; t += Time.deltaTime / UNITS_SPEED)
                    {
                        unit.unitTransform.position = Vector3.Lerp(unit.unitTransform.position, currentCellPos, t); // translation progressive de l'unité vers sa destination
                        yield return null;
                    }
                    unit.unitTransform.transform.position = currentCellPos;
                }
                else
                {
                    yield break; // arrête le déplacement pour ce tour
                }

                lastCell = currentCell;
                grid.RevealTilesInRadius(currentCell.offsetCoordinates, unit.unitType.tileRevealRadius, false); // révéler les cases "découvertes" par l'unité
            }

            if (cellToAttack != null)
            {
                UnitFight(currentCell, cellToAttack);
            }
        }
        else // pareil que ci-dessus, pour les unités civiles
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

    // déplacement d'une unité, renvoie [si il faut arrêter le déplacement] (booléen)
    public bool MoveQueuedUnit(int unitId)
    {
        queuedMovementData movementData = queuedUnitMovements[unitId];
        List<HexCell> path = movementData.path;
        Unit unit = units[unitId];

        Queue<HexCell> nextMoves = new Queue<HexCell>();
        nextMoves.Enqueue(path[0]);

        if (movementData.unitToAttackId!=-1 && (path[path.Count - 1].militaryUnit == null || path[path.Count-1].militaryUnit.id != movementData.unitToAttackId)) // si l'unité à attaquer a bougé, on annule le déplacement
        {
            return true;
        }

        float pathCost = 0f;
        Debug.Log(movementData.unitToAttackId + " " + GetDistance(path[0], path[path.Count - 1]) +" "+ unit.movesDone);
        while (path.Count>1 && unit.movesDone + pathCost + path[1].terrainType.terrainCost <= unit.unitType.MoveReach && (movementData.unitToAttackId==-1 || GetDistance(path[0], path[path.Count-1]) > unit.GetUnitMilitaryData().AttackRange)) // tant que l'unité peut se déplacer et qu'on n'est pas à portée de l'unité à attaquer
        {
            nextMoves.Enqueue(path[1]); // mettre dans la file la case sur laquelle on doit aller
            pathCost += path[1].terrainType.terrainCost;
            path.RemoveAt(0);
        }

        if (movementData.unitToAttackId != -1 && GetDistance(path[0], path[path.Count - 1]) <= unit.GetUnitMilitaryData().AttackRange)
        {
            StartCoroutine(MoveUnit(nextMoves, unit, path[path.Count-1])); // faire le déplacement et attaquer l'unité ennemie
        }
        else
        {
            StartCoroutine(MoveUnit(nextMoves, unit, null)); // faire le déplacement
        }
        unit.movesDone += pathCost;

        if (path.Count <= 1)
        {
            return true; // si on est arrivés à destination, on arrête le déplacement
        }
        return false;
    }

    // ajouter à la liste d'attente un déplacement d'unité
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
                movementData.unitToAttackId = destCell.militaryUnit.id; // si le déplacement finit sur une case occupé, il faut désigner l'unité de cette case comme ennemi à attaquer
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
        movementData.path = path; // obtenir le chemin jusqu'à la destination

        if (queuedUnitMovements.ContainsKey(unit.id))
        {
            queuedUnitMovements[unit.id] = movementData; // si l'unité est déjà dans la liste d'attente, on met à jour sa destination
        }
        else
        {
            queuedUnitMovements.Add(unit.id, movementData); // on ajoute à la liste d'attente l'unité et son déplacement
        }
            
        if (MoveQueuedUnit(unit.id))
        {
            queuedUnitMovements.Remove(unit.id); // si le mouvement était instantané, on retire de la liste d'attente ce qu'on vient d'y rajouter
            return true;
        }
        return false;
    }

    // combat entre les unités de deux cases
    private void UnitFight(HexCell attackerCell, HexCell defenderCell)
    {
        float cellDistance = GetDistance(attackerCell, defenderCell);

        defenderCell.militaryUnit.TakeDamage(attackerCell.militaryUnit.GetUnitMilitaryData().AttackPower); // l'unité défensive prend des dégats

        if(cellDistance <= defenderCell.militaryUnit.GetUnitMilitaryData().AttackRange)
        {
            attackerCell.militaryUnit.TakeDamage(defenderCell.militaryUnit.GetUnitMilitaryData().DefensePower); // l'unité offensive prend des dégats si à portée de l'unité défensive
        }

        if (!attackerCell.militaryUnit.IsAlive())
        {
            RemoveUnit(UnitType.UnitCategory.military, attackerCell); // supprimer l'unité offensive si morte
        }
        if (!defenderCell.militaryUnit.IsAlive())
        {
            RemoveUnit(UnitType.UnitCategory.military, defenderCell); // supprimer l'unité défensive si morte
        }
    }

    // enlever une unité de la case
    public void RemoveUnit(UnitType.UnitCategory unitCategory, HexCell unitCell)
    {
        Unit unit;
        if (unitCategory == UnitType.UnitCategory.military)
        {
            unit = unitCell.militaryUnit;
            unitCell.militaryUnit = null; // retirer l'unité de la case
        }
        else
        {
            unit = unitCell.civilianUnit;
            unitCell.civilianUnit = null; // retirer l'untié de la case
        }

        units.Remove(unit.id);
        Destroy(unit.unitPin.gameObject);
        Destroy(unit.unitTransform.gameObject); // supprimer l'instance de l'unité
    }

    // ajotuer un type d'unité à une case
    public void AddUnit(UnitType unitType, HexCell cell, Player master)
    {
        if(unitType.unitCategory == UnitType.UnitCategory.military)
        {
            if(cell.militaryUnit == null && IsCellTraversable(cell.terrainType, unitType))
            {
                Transform unitTransform = Instantiate( // instancier l'unité sur la case
                    unitType.unitPrefab.transform,
                    new Vector3(cell.tile.position.x, cell.terrainHigh, cell.tile.position.z),
                    new Quaternion(0, 0, 0, 1), 
                    unitContainer);
                Transform unitPinTransform = Instantiate(unitPinPrefab.transform, unitPinCanvas); // instancier le pin de l'unité sur l'unité

                UnitPin unitPin = unitPinTransform.GetComponent<UnitPin>();

                Unit unit = new Unit(unitTransform, unitType, unitPin, master); // créer l'instance de la classe unit associée à l'unité

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
            if (cell.militaryUnit == null && IsCellTraversable(cell.terrainType, unitType))
            {
                Transform unitTransform = Instantiate( // instancier l'unité sur la case
                    unitType.unitPrefab.transform,
                    new Vector3(cell.tile.position.x, cell.terrainHigh, cell.tile.position.z),
                    new Quaternion(0, 0, 0, 1),
                    unitContainer);
                Transform unitPinTransform = Instantiate(unitPinPrefab.transform, unitPinCanvas); // instancier le pin de l'unité sur l'unité

                UnitPin unitPin = unitPinTransform.GetComponent<UnitPin>();

                Unit unit = new Unit(unitTransform, unitType, unitPin, master); // créer l'instance de la classe unit associée à l'unité

                cell.civilianUnit = unit;
                units.Add(unit.id, unit);
            }
            else
            {
                Debug.LogError("trying to add a support unit on an already occupied tile");
            }
        }
    }

    // renvoie une liste de cases qui correspond au chemin le plus court entre deux cases
    private List<HexCell> GetShortestPath(HexCell startCell, HexCell finishCell, UnitType unitType, float heuristicFactor)
    {
        bool endCellFound = false;
        List<HexCell> pathCoordinates = new List<HexCell>();
        CellData startCellData = CreateCellData(null, startCell, finishCell, heuristicFactor);

        List<CellData> visitedCells = new List<CellData>();
        List<CellData> cellsToVisit = new List<CellData>() { startCellData };

        CellData currentCellData = null;

        //System.DateTime startTime = System.DateTime.Now;

        int iterations = 0;
        while (!endCellFound && cellsToVisit.Count > 0 && iterations < MAX_ITERATIONS) // tant qu'il reste des cases à visiter et qu'on a pas trouvé la case de fin
        {
            currentCellData = cellsToVisit[0];
            AddNewCellData(currentCellData, visitedCells); // choisit la case avec le coût le plus faible
            cellsToVisit.RemoveAt(0);

            for (int i = 0; i < 6; i++) // cycle dans les voisins de la case actuelle
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
                else if (IsCellTraversable(currentCellData.cell.neighbours[i].terrainType, unitType)) // si la case est traversable
                {
                    CellData currentCellNeighboursData = CreateCellData(currentCellData, currentCellData.cell.neighbours[i], finishCell, heuristicFactor);
                    
                    int isCellDataVisited = GetCellDataIndex(currentCellNeighboursData, visitedCells); // l'indice du voisin actuel dans les cases visitées
                    if (isCellDataVisited == -1)
                    {
                        int isCellDataInToVisit = GetCellDataIndex(currentCellNeighboursData, cellsToVisit); // l'indice du voisin actuel dans les cases à visiter
                        if (isCellDataInToVisit == -1)
                        {
                            AddNewCellData(currentCellNeighboursData, cellsToVisit); // ajouter le voisin dans les cases à visiter
                        }
                        else if (currentCellNeighboursData.FCost < cellsToVisit[isCellDataInToVisit].FCost)
                        {
                            cellsToVisit.RemoveAt(isCellDataInToVisit);
                            AddNewCellData(currentCellNeighboursData, cellsToVisit); // mettre à jour le voisin dans les cases à visiter
                        }
                    }
                    else if (currentCellNeighboursData.FCost < visitedCells[isCellDataVisited].FCost)
                    {
                        visitedCells.RemoveAt(isCellDataVisited);
                        AddNewCellData(currentCellNeighboursData, visitedCells); // mettre à jour le voisin dans les cases visitées
                    }
                    
                }
            }

            iterations++;
        }
        

        if (endCellFound)
        {
            currentCellData = CreateCellData(currentCellData, finishCell, finishCell, 0f);
            while (currentCellData.cell.offsetCoordinates != startCell.offsetCoordinates) // refaire le chemin en sens inverse avec les cases précédentes
            {
                pathCoordinates.Insert(0, currentCellData.cell);
                currentCellData = currentCellData.parentCellData;
            }
            pathCoordinates.Insert(0, startCell);

            System.DateTime endTime = System.DateTime.Now;
            //Debug.Log("FOUND PATH FROM " + startCell.offsetCoordinates + " TO " + finishCell.offsetCoordinates + " IN " + endTime.Subtract(startTime) + "s AND " + iterations + " iterations");
            
            return pathCoordinates;
        }
        else
        {
            System.DateTime endTime = System.DateTime.Now;
            //Debug.Log("NO PATH FOUND FROM " + startCell.offsetCoordinates + " TO " + finishCell.offsetCoordinates + " IN " + endTime.Subtract(startTime) + "s AND " + iterations + " iterations");
            return null;
        }
    }

    // rajoute un élément à une liste triée
    private void AddNewCellData(CellData cellData, List<CellData> cellDataList) // A REFAIRE EN DICHOTOMIE
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

    // recherche séquentielle d'un élément dans une liste triée
    private int GetCellDataIndex(CellData cellData, List<CellData> cellDataList) // A REFAIRE EN DICHOTOMIE
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

    // renvoie la distance entre deux cases (en nombre de cases, pas distance brute)
    public float GetDistance(HexCell cell1, HexCell cell2)
    {
        float euclideanDistance = Vector3.Distance(cell1.cubeCoordinates, cell2.cubeCoordinates)/grid.hexSize;
        float realDistance = euclideanDistance/Mathf.Sqrt(2);
        return Mathf.Round(realDistance * 100) / 100;
    }

    // renvoie un CellData complet
    private CellData CreateCellData(CellData parentCellData, HexCell cell, HexCell destCell, float heuristicFactor)
    {
        float GCost = GetDistance(cell, destCell);
        float HCost = cell.terrainType.terrainCost * HEURISTIC_SCALING;
        float FCost = GCost + HCost * heuristicFactor;
        return new CellData(GCost, HCost, FCost, cell, parentCellData);
    }

    // renvoie si la case est traversable par l'unité ou non
    private bool IsCellTraversable(TerrainType terrainType, UnitType unitType)
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

    // classe utile pour stocker les données de chaque case dans le cadre de l'algorithme A*
    private class CellData
    {
        private readonly float GCost; // cout de déplacement brut
        public readonly float HCost; // cout de déplacement heuristique
        public readonly float FCost; // cout de déplacement total
        public readonly HexCell cell; // case
        public readonly CellData parentCellData; // précédent

        // constructeur de la classe
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

// classe qui stocke les données d'une unité
public class Unit
{
    public readonly int id; // id
    public Transform unitTransform; // le transform de l'unité
    public readonly UnitType unitType; // le type d'unité (classe générale)
    public readonly Player master; // le maître de l'unité (le joueur ou l'IA)
    public readonly UnitPin unitPin; // le pin de l'unité (pour le repérer sur la carte)
    public string unitName; // le nom de l'unité (personnalisable)

    private MilitaryUnitType militaryUnitType; // type de l'unité spécial militaire
    private CivilianUnitType civilianUnitType; // type de l'unité spécial civil
    private float currentHealth; // vie actuelle de l'unité

    public float movesDone; // le nombre de déplacements effectués ce tour
    public int lastDamagingTurn { get; private set; } // le dernier où l'unité a subi des dégats

    // constructeur de la classe
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

        if (unitType.unitCategory == UnitType.UnitCategory.military)
        {
            this.militaryUnitType = unitType as MilitaryUnitType;
            this.currentHealth = this.militaryUnitType.MaxHealth;
        }
    }

    // prendre des dégats
    public void TakeDamage(float damage)
    {
        this.currentHealth -= damage;
        this.unitPin.UpdateHealth(this.currentHealth, this.militaryUnitType.MaxHealth);
        lastDamagingTurn = TurnManager.instance.currentTurn;
    }

    // se soigner
    public void Heal(float healAmount)
    {
        this.currentHealth += healAmount;
        
        if(this.currentHealth >= this.militaryUnitType.MaxHealth)
        {
            this.currentHealth = this.militaryUnitType.MaxHealth;
        }

        this.unitPin.UpdateHealth(this.currentHealth, this.militaryUnitType.MaxHealth);
    }

    // l'unité est-elle en vie
    public bool IsAlive()
    {
        return this.currentHealth >= 0;
    }

    // obtenir les données militaires de l'unité
    public MilitaryUnitType GetUnitMilitaryData()
    {
        return this.militaryUnitType;
    }
}

// struct pour enregistrer les éléments du mouvement d'une unité
public struct queuedMovementData
{
    public int unitToAttackId;
    public List<HexCell> path;
}