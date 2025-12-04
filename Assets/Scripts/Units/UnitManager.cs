using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField] HexGrid grid;
    [SerializeField] GameObject unitPinPrefab;
    [SerializeField] Transform unitPinCanvas;
    [SerializeField] Transform unitContainer;

    public MilitaryUnitType[] militaryUnits;
    public CivilianUnitType[] civilianUnits;

    public Building[] buildings;

    [HideInInspector] public int nextAvailableId = 1;
    public Dictionary<int, Unit> units { get; private set; }
    public Dictionary<int, queuedMovementData> queuedUnitMovements = new Dictionary<int, queuedMovementData>();
    public float lastDistance = 1;
    public int movingUnitsCount = 0;

    private float heuristicScaling = 1f;
    private float heuristicFactor = 1f;
    private int maxIterations = 10000;
    private float unitMoveSpeed = 4f;
    private float unitRotationSpeed =540f;
    private float coUnitsScaleFactor = 0.7f;
    private Vector3 coUnitsOffset = new Vector3(0.4f, 0, 0);
    [HideInInspector] public string[] NAMES_LIST = { "Abel", "Achille", "Adam", "Adolphe", "Adrien", "Aimable", "Aimé", "Alain", "Alan", "Alban", "Albert", "Albin", "Alex", 
        "Alexandre", "Alexis", "Alfred", "Aliaume", "Alix", "Aloïs", "Alphonse", "Amaury", "Ambroise", "Amédée", "Amour", "Ananie", "Anastase", "Anatole", "André", "Andréa", 
        "Ange", "Anicet", "Anselme", "Antelme", "Anthelme", "Anthony", "Antoine", "Antonin", "Apollinaire", "Ariel", "Aristide", "Armand", "Armel", "Arnaud", "Arsène", "Arthur",
        "Aubin", "Auguste", "Augustin", "Aurélien", "Axel", "Aymard", "Aymeric", "Balthazar", "Baptiste", "Baptistin", "Barnabé", "Barnard", "Barthélémy", "Basile", "Bastien", 
        "Baudouin", "Benjamin", "Benoît", "Bérenger", "Bernard", "Bernardin", "Bertrand", "Bienvenu", "Blaise", "Boris", "Briac", "Brice", "Bruno", "Calixte", "Camille", 
        "Casimir", "Cédric", "Céleste", "Célestin", "César", "Charles", "Charlie", "Christian", "Christophe", "Claude", "Clément", "Clovis", "Colin", "Côme", "Constant", 
        "Constantin", "Corentin", "Crépin", "Cyprien", "Cyril", "Cyrille", "Damien", "Daniel", "Dany", "David", "Davy", "Denis", "Désiré", "Didier", "Dimitri", "Dominique", 
        "Donald", "Donatien", "Dorian", "Eden", "Edgar", "Edgard", "Edmond", "Edouard", "Elias", "Elie", "Eloi", "Emile", "Emilien", "Emmanuel", "Eric", "Ernest", "Erwan", 
        "Erwann", "Etienne", "Eudes", "Eugène", "Evrard", "abien", "Fabrice", "Faustin", "Félicien", "Félix", "Ferdinand", "Fernand", "Fiacre", "Fidèle", "Firmin", "Flavien", 
        "Florent", "Florentin", "Florian", "Floribert", "Fortuné", "Francis", "Franck", "François", "Frédéric", "Fulbert", "Gabin", "Gabriel", "Gaël", "Gaétan", "Gaëtan", 
        "Gaspard", "Gaston", "Gatien", "Gauthier", "Gautier", "Geoffroy", "Georges", "Gérald", "Gérard", "Géraud", "Germain", "Gervais", "Ghislain", "Gilbert", "Gildas",
        "Gilles", "Godefroy", "Goeffrey", "Gontran", "Gonzague", "Gratien", "Grégoire", "Gregory", "Guénolé", "Guilain", "Guilem", "Guillaume", "Gustave", "Guy", "Guylain", 
        "Gwenaël", "Gwendal", "Habib", "Hadrien", "Hector", "Henri", "Herbert", "Hercule", "Hermann", "Hervé", "Hippolythe", "Honoré", "Honorin", "Horace", "Hubert", 
        "Hugo", "Hugues", "Hyacinthe", "Ignace", "Igor", "Isidore", "Ismaël", "Jacky", "Jacob", "Jacques", "Jean", "Jérémie", "Jérémy", "Jérôme", "Joachim", "Jocelyn",
        "Joël", "Johan", "Jonas", "Jonathan", "Jordan", "José", "Joseph", "Joshua", "Josselin", "Josué", "Judicaël", "Jules", "Julian", "Julien", "Juste", "Justin",
        "Kévin", "Lambert", "Lancelot", "Landry", "Laurent", "Lazare", "Léandre", "Léger", "Léo", "Léon", "Léonard", "Léonce", "Léopold", "Lilian", "Lionel", 
        "Loan", "Loïc", "Loïck", "Loris", "Louis", "Louison", "Loup", "Luc", "Luca", "Lucas", "Lucien", "Ludovic", "Maël", "Mahé", "Maixent", "Malo", "Manuel", 
        "Marc", "Marceau", "Marcel", "Marcelin", "Marcellin", "Marin", "Marius", "Martial", "Martin", "Martinien", "Matéo", "Mathéo", "Mathias", "Mathieu", 
        "Mathis", "Mathurin", "Mathys", "Mattéo", "Matthias", "Matthieu", "Maurice", "Maxence", "Maxime", "Maximilien", "Médard", "Melchior", "Merlin", 
        "Michel", "Milo", "Modeste", "Morgan", "Naël", "Narcisse", "Nathan", "Nathanaël", "Nestor", "Nicolas", "Noa", "Noah", "Noé", "Noël", "Norbert", 
        "Octave", "Octavien", "Odilon", "Olivier", "Omer", "Oscar", "Pacôme", "Parfait", "Pascal", "Patrice", "Patrick", "Paul", "Paulin", "Perceval", 
        "Philémon", "Philibert", "Philippe", "Pierre", "Pierrick", "Prosper", "Quentin", "Rafaël", "Raoul", "Raphaël", "Raymond", "Réginald", "Régis", 
        "Rémi", "Rémy", "Renaud", "René", "Reynald", "Richard", "Robert", "Robin", "Rodolphe", "Rodrigue", "Roger", "Roland", "Romain", "Romaric", "Roméo",
        "Romuald", "Ronan", "Sacha", "Salomon", "Sam", "Sami", "Samson", "Samuel", "Samy", "Sasha", "Saturnin", "Sébastien", "Séraphin", "Serge", "Séverin",
        "Sidoine", "Siméon", "Simon", "Sixte", "Stanislas", "Stéphane", "Sylvain", "Sylvère", "Sylvestre", "Tancrède", "Tanguy", "Théo", "Théodore", "Théophane", 
        "Théophile", "Thibaud", "Thibaut", "Thierry", "Thilbault", "Thomas", "Tibère", "Timéo", "Timothé", "Timothée", "Titouan", "Tristan", "Tyméo", "Ulrich", "Ulysse", 
        "Urbain", "Uriel", "Valentin", "Valère", "Valérien", "Valéry", "Valmont", "Venceslas", "Vianney", "Victor", "Victorien", "Vincent", "Virgile", "Vivien", "Wilfrid", 
        "William", "Xavier", "Yaël", "Yanis", "Yann", "Yannick", "Yohan", "Yves", "Yvon", "Yvonnick", "Zacharie", "Zéphirin" };

    [Header("UI")]
    public Transform unitActionsPanel;
    public Transform unitActionsContainer;
    public Transform unitActionsButtonTemplate;

    public static UnitManager instance;

    private void Awake()
    {
        grid.OnCellInstancesGenerated += OnCellLoaded;
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de UnitManager dans la scene");
            return;
        }
        instance = this;

        militaryUnits = Resources.LoadAll<MilitaryUnitType>("Units/MilitaryUnit");
        civilianUnits = Resources.LoadAll<CivilianUnitType>("Units/CivilianUnit");
        units = new Dictionary<int, Unit>();
    }

    public void Start()
    {
        TurnManager.instance.OnTurnChange += UpdateUnits; // ajoute UpdateUnits à la liste des fonctions appelées à chaquez début de tour

        unitMoveSpeed *= grid.hexSize;
        unitRotationSpeed *= grid.hexSize;

        maxIterations = grid.gridSize.height * grid.gridSize.width;

        HideActionPanel();
    }

    public void UpdatePinsScale(float newDistance)
    {
        lastDistance = newDistance;
        foreach(var unit in units.Values)
        {
            unit.unitPin.UpdateScale(newDistance);
        }
    }

    private void CheckCellUnitsConflict(HexCell cell)
    {
        if(cell.militaryUnit != null && cell.civilianUnit != null)
        {
            cell.militaryUnit.unitTransform.localScale = Vector3.one * coUnitsScaleFactor;
            cell.civilianUnit.unitTransform.localScale = Vector3.one * coUnitsScaleFactor;

            cell.militaryUnit.ApplyNewOffset(coUnitsOffset * grid.hexSize);
            cell.civilianUnit.ApplyNewOffset(coUnitsOffset * grid.hexSize * -1);
        }
    }

    public UnitType GetUnitType(string unitTypeName)
    {
        foreach(var unit in militaryUnits)
        {
            if (unit.name == unitTypeName)
            {
                return unit;
            }
        }
        foreach(var unit in civilianUnits)
        {
            if (unit.name == unitTypeName)
            {
                return unit;
            }
        }

        return null;
    }

    private void OnCellLoaded()
    {
        SaveData data = SaveManager.instance.lastSave;

        if (data != null)
        {
            foreach(var unitData in data.units)
            {
                Unit unit = AddUnit(unitData.unitType, grid.GetTile(unitData.cellCoordinates), PlayerManager.instance.playerEntities[unitData.masterId]);
                unit.currentHealth = unitData.currentHealth;
                unit.movesDone = unitData.movesDone;
                unit.lastDamagingTurn = unitData.lastDamagingTurn;
                unit.chargesLeft = unitData.chargesLeft;

                if (unitData.queuedMovementData.path.Count > 1) 
                {
                    queuedMovementData movementData = unitData.queuedMovementData;
                    for(int i = 0; i < movementData.path.Count; i++)
                    {
                        movementData.path[i] = grid.GetTile(movementData.path[i].offsetCoordinates);
                    }

                    queuedUnitMovements.Add(unit.id, movementData); 
                }
            }
        }
    }

    public void ShowActionPanel(CivilianUnitType type, HexCell cell)
    {
        foreach (Transform child in unitActionsContainer.transform)
        {
            Destroy(child.gameObject);
        }

        foreach(Building building in type.buildableBuildings)
        {
            BuildActionButton actionButton = Instantiate(unitActionsButtonTemplate, unitActionsContainer).GetComponent<BuildActionButton>();
            actionButton.building = building;

            actionButton.icon.sprite = building.buildingSprite;
            actionButton.button.onClick.AddListener(delegate { CivilianUnitAction(SelectionManager.instance.selectedCell, building.buildingName); UpdateActionPanel(cell); });
        }

        UpdateActionPanel(cell);

        unitActionsPanel.gameObject.SetActive(true);
    }

    public void UpdateActionPanel(HexCell cell)
    {
        foreach(Transform child in unitActionsContainer.transform)
        {
            BuildActionButton button = child.GetComponent<BuildActionButton>();
            button.UpdateButton(cell);
        }
    }

    public void HideActionPanel()
    {
        unitActionsPanel.gameObject.SetActive(false);
    }

    public void CivilianUnitAction(HexCell cell, Building.BuildingNames buildingName)
    {
        if(cell.civilianUnit == null)
        {
            Debug.LogError("trying to execute action with no selected unit ?!");
            return;
        }

        if (cell.CreateBuilding(grid.GetBuilding(buildingName), cell.civilianUnit))
        {
            if (cell.civilianUnit.ConsumeCharge())
            {
                RemoveUnit(UnitType.UnitCategory.civilian, cell);
                CheckCellUnitsConflict(cell);
                cell.civilianUnit = null;
            }
        }
    }

    // combat entre une unité et une cité
    public void CityFight(HexCell unitCell, HexCell cityCell)
    {
        float cellDistance = GetDistance(unitCell, cityCell);
        City city = CityManager.instance.cities[cityCell.offsetCoordinates];

        if (cityCell.militaryUnit != null && cellDistance <= cityCell.militaryUnit.GetUnitMilitaryData().AttackRange)
        {
            unitCell.militaryUnit.TakeDamage(cityCell.militaryUnit.GetUnitMilitaryData().AttackPower); // l'unité offensive prend des dégats si à portée de l'unité défensive
        }

        city.TakeDamage(unitCell.militaryUnit); // la cité prend des dégats (mitigés par son degré de protection)

        if (!unitCell.militaryUnit.IsAlive())
        {
            RemoveUnit(UnitType.UnitCategory.military, unitCell); // supprimer l'unité offensive si morte
        }
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
            if (MoveQueuedUnit(unitId).movementFinished) // déplace l'unité
            {
                finishedMovements.Add(unitId); // s'il faut arrêter le mouvement, ajoute l'id de l'unité dans la liste des unités à arrêter de déplacer
            }
        }

        foreach (var unitId in finishedMovements)
        {
            queuedUnitMovements.Remove(unitId); // enlever l'unité de la liste d'attente de déplacements
        }
    }

    public IEnumerator MoveUnit(Queue<HexCell> nextMoves, Unit unit, HexCell cellToAttack, bool finishedMovement)
    {
        HexCell lastCell = nextMoves.Dequeue();
        HexCell nextCell = lastCell;

        movingUnitsCount++;

        if (unit.unitType.unitCategory == UnitType.UnitCategory.military)
        {
            while (nextMoves.Count > 0)
            {
                nextCell = nextMoves.Dequeue();

                if (nextCell.militaryUnit == null || queuedUnitMovements.ContainsKey(nextCell.militaryUnit.id))
                {
                    nextCell.militaryUnit = unit;
                    lastCell.militaryUnit = null;

                    Vector3 nextCellPos = new Vector3(nextCell.tile.position.x, nextCell.terrainHigh, nextCell.tile.position.z);

                    Vector3 nextCellDir = nextCellPos - unit.unitTransform.position;
                    Quaternion rot = Quaternion.LookRotation(nextCellDir);
                    while (unit.unitTransform.rotation != rot)
                    {
                        unit.unitTransform.rotation = Quaternion.RotateTowards(unit.unitTransform.rotation, rot, Time.deltaTime * unitRotationSpeed);
                        yield return null;
                    }

                    Vector3 nextCellScale = Vector3.one;
                    Vector3 nextCellOffset = Vector3.zero;
                    if (nextCell.civilianUnit != null)
                    {
                        nextCellScale *= coUnitsScaleFactor;
                        nextCellOffset = coUnitsOffset * grid.hexSize;
                        nextCellPos += nextCellOffset;
                    }

                    Unit nextCellCivilianUnit = null;
                    if (nextCell.civilianUnit != null && !queuedUnitMovements.ContainsKey(nextCell.civilianUnit.id))
                    {
                        nextCellCivilianUnit = nextCell.civilianUnit;
                    }

                    Unit lastCellCivilianUnit = null;
                    if (lastCell.civilianUnit != null && !queuedUnitMovements.ContainsKey(lastCell.civilianUnit.id))
                    {
                        lastCellCivilianUnit = lastCell.civilianUnit;
                    }

                    while (unit.unitTransform.position != nextCellPos)
                    {
                        unit.unitTransform.position = Vector3.MoveTowards(unit.unitTransform.position, nextCellPos, Time.deltaTime * unitMoveSpeed);
                        unit.unitTransform.localScale = Vector3.MoveTowards(unit.unitTransform.localScale, nextCellScale * grid.hexSize, Time.deltaTime * unitMoveSpeed * 0.3f);

                        if (nextCellCivilianUnit != null)
                        {
                            nextCellCivilianUnit.ApplyNewOffset(Vector3.MoveTowards(nextCellCivilianUnit.unitOffset, coUnitsOffset * grid.hexSize * -1, Time.deltaTime * unitMoveSpeed * coUnitsOffset.magnitude));
                            nextCellCivilianUnit.unitTransform.localScale = Vector3.MoveTowards(nextCellCivilianUnit.unitTransform.localScale, nextCellScale * grid.hexSize, Time.deltaTime * unitMoveSpeed * 0.3f);
                        }
                        if (lastCellCivilianUnit != null)
                        {
                            lastCellCivilianUnit.ApplyNewOffset(Vector3.MoveTowards(lastCellCivilianUnit.unitOffset, Vector3.zero, Time.deltaTime * unitMoveSpeed * coUnitsOffset.magnitude));
                            lastCellCivilianUnit.unitTransform.localScale = Vector3.MoveTowards(lastCellCivilianUnit.unitTransform.localScale, Vector3.one * grid.hexSize, Time.deltaTime * unitMoveSpeed * 0.3f);
                        }

                        yield return null;
                    }
                    unit.unitOffset = nextCellOffset;

                    if(unit.master == PlayerManager.instance.player)
                    {
                        grid.RevealTilesInRadius(nextCell.offsetCoordinates, unit.unitType.sightRadius, SelectionManager.instance.showOverlay, true); // révéler les cases "découvertes" par l'unité
                        grid.UpdateActiveTiles(); // mettre à jour les cases découvertes par l'unité
                    }
                }
                else
                {
                    yield break; // arrête le déplacement pour ce tour
                }

                lastCell = nextCell;
            }

            if (cellToAttack != null)
            {
                if (cellToAttack.building.buildingName == Building.BuildingNames.City)
                {
                    CityFight(nextCell, cellToAttack);
                }
                else
                {
                    UnitFight(nextCell, cellToAttack);
                }
            }
        }
        else
        {
            while (nextMoves.Count > 0)
            {
                nextCell = nextMoves.Dequeue();

                if (nextCell.civilianUnit == null || queuedUnitMovements.ContainsKey(nextCell.civilianUnit.id))
                {
                    nextCell.civilianUnit = unit;
                    lastCell.civilianUnit = null;

                    Vector3 nextCellPos = new Vector3(nextCell.tile.position.x, nextCell.terrainHigh, nextCell.tile.position.z);

                    Vector3 nextCellDir = nextCellPos - unit.unitTransform.position;
                    Quaternion rot = Quaternion.LookRotation(nextCellDir);
                    while (unit.unitTransform.rotation != rot)
                    {
                        unit.unitTransform.rotation = Quaternion.RotateTowards(unit.unitTransform.rotation, rot, Time.deltaTime * unitRotationSpeed);
                        yield return null;
                    }

                    Vector3 nextCellScale = Vector3.one;
                    Vector3 nextCellOffset = Vector3.zero;
                    if (nextCell.militaryUnit != null)
                    {
                        nextCellScale *= coUnitsScaleFactor;
                        nextCellOffset = coUnitsOffset * grid.hexSize * -1;
                        nextCellPos += nextCellOffset;
                    }

                    Unit nextCellMilitaryUnit = null;
                    if (nextCell.militaryUnit != null && !queuedUnitMovements.ContainsKey(nextCell.militaryUnit.id))
                    {
                        nextCellMilitaryUnit = nextCell.militaryUnit;
                    }

                    Unit lastCellMilitaryUnit = null;
                    if (lastCell.militaryUnit != null && !queuedUnitMovements.ContainsKey(lastCell.militaryUnit.id))
                    {
                        lastCellMilitaryUnit = lastCell.militaryUnit;
                    }

                    while (unit.unitTransform.position != nextCellPos)
                    {
                        unit.unitTransform.position = Vector3.MoveTowards(unit.unitTransform.position, nextCellPos, Time.deltaTime * unitMoveSpeed);
                        unit.unitTransform.localScale = Vector3.MoveTowards(unit.unitTransform.localScale, nextCellScale * grid.hexSize, Time.deltaTime * unitMoveSpeed * 0.3f);

                        if (nextCellMilitaryUnit != null)
                        {
                            nextCellMilitaryUnit.ApplyNewOffset(Vector3.MoveTowards(nextCellMilitaryUnit.unitOffset, coUnitsOffset * grid.hexSize, Time.deltaTime * unitMoveSpeed * coUnitsOffset.magnitude));
                            nextCellMilitaryUnit.unitTransform.localScale = Vector3.MoveTowards(nextCellMilitaryUnit.unitTransform.localScale, nextCellScale * grid.hexSize, Time.deltaTime * unitMoveSpeed * 0.3f);
                        }
                        if (lastCellMilitaryUnit != null)
                        {
                            lastCellMilitaryUnit.ApplyNewOffset(Vector3.MoveTowards(lastCellMilitaryUnit.unitOffset, Vector3.zero, Time.deltaTime * unitMoveSpeed * coUnitsOffset.magnitude));
                            lastCellMilitaryUnit.unitTransform.localScale = Vector3.MoveTowards(lastCellMilitaryUnit.unitTransform.localScale, Vector3.one * grid.hexSize, Time.deltaTime * unitMoveSpeed * 0.3f);
                        }

                        yield return null;
                    }
                    unit.unitOffset = nextCellOffset;

                    if(PlayerManager.instance.player == unit.master)
                    {
                        grid.UpdateActiveTiles(); // mettre à jour les cases découvertes par l'unité
                        grid.RevealTilesInRadius(nextCell.offsetCoordinates, unit.unitType.sightRadius, SelectionManager.instance.showOverlay, true); // révéler les cases "découvertes" par l'unité
                    }
                }
                else
                {
                    yield break; // arrête le déplacement pour ce tour
                }

                lastCell = nextCell;
            }

            UpdateActionPanel(lastCell);
        

        if (finishedMovement)
        {
            queuedUnitMovements[unit.id].unitAction.Invoke();
        }

        movingUnitsCount--;
    }

    // déplacement d'une unité, renvoie [si il faut arrêter le déplacement] (booléen)
    public FirstMovementData MoveQueuedUnit(int unitId)
    {
        queuedMovementData movementData = queuedUnitMovements[unitId];
        List<HexCell> path = movementData.path;
        Unit unit = units[unitId];

        FirstMovementData dataToReturn = new FirstMovementData();
        dataToReturn.unitCell = path[0];

        Queue<HexCell> nextMoves = new Queue<HexCell>();
        nextMoves.Enqueue(path[0]);

        if (movementData.unitToAttackId!=-1 && (path[path.Count - 1].militaryUnit == null || path[path.Count-1].militaryUnit.id != movementData.unitToAttackId)) // si l'unité à attaquer a bougé, on annule le déplacement
        {
            dataToReturn.movementFinished = true;
            return dataToReturn;
        }
        if(movementData.attacksACity && CityManager.instance.cities[path[path.Count-1].offsetCoordinates].master == unit.master)
        {
            dataToReturn.movementFinished = true;
            return dataToReturn;
        }

        float pathCost = 0f;
        while (path.Count>1 && IsCellTraversable(path[1], unit.unitType, false) && (unit.movesDone + pathCost + path[1].terrainType.terrainCost <= unit.unitType.MoveReach  || unit.unitType.speciallyAccessibleTerrains.Contains(path[1].terrainType)) && ((movementData.unitToAttackId==-1 && !movementData.attacksACity)|| GetDistance(path[0], path[path.Count-1]) > unit.GetUnitMilitaryData().AttackRange)) // tant que l'unité peut se déplacer et qu'on n'est pas à portée de l'unité à attaquer
        {
            nextMoves.Enqueue(path[1]); // mettre dans la file la case sur laquelle on doit aller
            pathCost += path[1].terrainType.terrainCost;
            path.RemoveAt(0);
        }

        if (path.Count <= 1 || !IsCellTraversable(path[1], unit.unitType, false))
        {
            dataToReturn.movementFinished = true;
        }
        else
        {
            dataToReturn.movementFinished = false;
        }

        if ((movementData.unitToAttackId != -1 && GetDistance(path[0], path[path.Count - 1]) <= unit.GetUnitMilitaryData().AttackRange) || (movementData.attacksACity == true && GetDistance(path[0], path[path.Count - 1]) <= unit.GetUnitMilitaryData().AttackRange))
        {
            StartCoroutine(MoveUnit(nextMoves, unit, path[path.Count-1], dataToReturn.movementFinished)); // faire le déplacement et attaquer l'unité ennemie
        }
        else
        {
            StartCoroutine(MoveUnit(nextMoves, unit, null, dataToReturn.movementFinished)); // faire le déplacement
        }
        unit.movesDone += pathCost;
        dataToReturn.unitCell = path[0];
        
        return dataToReturn;
    }

    // ajouter à la liste d'attente un déplacement d'unité
    public HexCell QueueUnitMovement(HexCell unitCell, HexCell destCell, UnitType.UnitCategory unitCategory, Action unitAction, bool isAI)
    {
        queuedMovementData movementData = new queuedMovementData();
        movementData.attacksACity = false;
        movementData.unitToAttackId = -1;

        Unit unit;
        if (unitCategory == UnitType.UnitCategory.military)
        {
            unit = unitCell.militaryUnit;
            if (destCell.building.buildingName == Building.BuildingNames.City && CityManager.instance.cities[destCell.offsetCoordinates].master != unitCell.militaryUnit.master)
            {
                movementData.attacksACity = true;
            }
            else if (destCell.militaryUnit != null && destCell.militaryUnit.master != unitCell.militaryUnit.master)
            {
                movementData.unitToAttackId = destCell.militaryUnit.id; // si le déplacement finit sur une case occupé, il faut désigner l'unité de cette case comme ennemi à attaquer
            }
        }
        else
        {
            unit = unitCell.civilianUnit;
        }

        List<HexCell> path = GetShortestPath(unitCell, destCell, unit.unitType, isAI);
        if (path == null)
        {
            return null;
        }
        if (path.Count <= 1)
        {
            if (queuedUnitMovements.ContainsKey(unit.id)) // utilisé si le joueur veut retirer l'unité de la liste d'attente
            {
                queuedUnitMovements.Remove(unit.id);
            }
            return destCell;
        }
        movementData.path = path; // obtenir le chemin jusqu'à la destination
        movementData.unitAction = unitAction;

        if (queuedUnitMovements.ContainsKey(unit.id))
        {
            queuedUnitMovements[unit.id] = movementData; // si l'unité est déjà dans la liste d'attente, on met à jour sa destination
        }
        else
        {
            queuedUnitMovements.Add(unit.id, movementData); // on ajoute à la liste d'attente l'unité et son déplacement
        }

        float beforeMoving = unit.movesDone;
        FirstMovementData newUnitCell = MoveQueuedUnit(unit.id);
        if (newUnitCell.movementFinished)
        {
            queuedUnitMovements.Remove(unit.id); // si le mouvement était instantané, on retire de la liste d'attente ce qu'on vient d'y rajouter
        }
        if(beforeMoving != unit.movesDone) // si il y a eu mouvement, on l'indique au selectionManager
        {
            return newUnitCell.unitCell;
        }
        return newUnitCell.unitCell;
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
            unitCell.civilianUnit = null; // retirer l'unité de la case

            unitActionsPanel.gameObject.SetActive(false);
        }

        units.Remove(unit.id);
        Destroy(unit.unitPin.gameObject);
        Destroy(unit.unitTransform.gameObject); // supprimer l'instance de l'unité
    }

    public Unit AddUnit(UnitType unitType, HexCell cell, Player master)
    {
        bool isMilitary = unitType.unitCategory == UnitType.UnitCategory.military;

        // Sélection
        Unit existingUnit = isMilitary ? cell.militaryUnit : cell.civilianUnit;

        if (existingUnit != null)
        {
            Debug.LogError($"Trying to add a {unitType.unitCategory} unit on an occupied tile: {cell.offsetCoordinates}");
            return null;
        }

        if (!IsCellTraversable(cell, unitType, master!=PlayerManager.instance.player))
        {
            Debug.LogError($"Trying to add unit {unitType.name} on non-traversable tile.");
            return null;
        }

        // Instantiation
        Transform unitTransform = Instantiate(
            unitType.unitPrefab.transform,
            new Vector3(cell.tile.transform.position.x, cell.terrainHigh, cell.tile.transform.position.z),
            Quaternion.identity,
            unitContainer);

        Transform unitPinTransform = Instantiate(unitPinPrefab.transform, unitPinCanvas);
        UnitPin unitPin = unitPinTransform.GetComponent<UnitPin>();

        // Vision
        if (master == PlayerManager.instance.player)
            grid.RevealTilesInRadius(cell.offsetCoordinates, unitType.sightRadius, SelectionManager.instance.showOverlay, true);

        // Création de l’unité
        Unit unit = new Unit(unitTransform, unitType, unitPin, master);

        // Visibilité
        if (!cell.isRevealed || !cell.isActive)
        {
            unitTransform.GetComponentInChildren<Renderer>().enabled = false;
            unitPin.gameObject.SetActive(false);
        }

        // Assignation
        if (isMilitary)
            cell.militaryUnit = unit;
        else
            cell.civilianUnit = unit;

        units.Add(unit.id, unit);

        CheckCellUnitsConflict(cell);

        return unit;
    }

    // renvoie une liste de cases qui correspond au chemin le plus court entre deux cases
    public List<HexCell> GetShortestPath(HexCell startCell, HexCell finishCell, UnitType unitType, bool isAI = false, int use = 0)
    {
        bool endCellFound = false;
        List<HexCell> pathCoordinates = new List<HexCell>();
        CellData startCellData = new CellData(null, startCell, finishCell);

        List<CellData> visitedCells = new List<CellData>();
        List<CellData> cellsToVisit = new List<CellData>() { startCellData };

        CellData currentCellData = null;

        if (!IsCellTraversable(finishCell, unitType, isAI)) { return null; }

        int iterations = 0;
        while (!endCellFound && cellsToVisit.Count > 0 && iterations < maxIterations) // tant qu'il reste des cases à visiter et qu'on a pas trouvé la case de fin
        {
            currentCellData = cellsToVisit[0];
            AddNewCellData(currentCellData, visitedCells, use); // choisit la case avec le coût le plus faible
            cellsToVisit.RemoveAt(0);

            for (int i = 0; i < 6; i++) // cycle dans les voisins de la case actuelle
            {
                if (currentCellData.cell.neighbours[i] == null)
                {
                    continue;
                }
                else if (currentCellData.cell.neighbours[i].offsetCoordinates == finishCell.offsetCoordinates) // si la case est celle de fin
                {
                    endCellFound = true;
                    break;
                }
                else if (IsCellTraversable(currentCellData.cell.neighbours[i], unitType, isAI)) // si la case est traversable
                {
                    CellData currentCellNeighboursData = new CellData(currentCellData, currentCellData.cell.neighbours[i], finishCell); // nouveau CellData pour le voisin actuel
                    
                    int isCellDataVisited = GetCellDataIndex(currentCellNeighboursData, visitedCells); // l'indice du voisin actuel dans les cases visitées
                    if (isCellDataVisited < 0)
                    {
                        int isCellDataInToVisit = GetCellDataIndex(currentCellNeighboursData, cellsToVisit); // l'indice du voisin actuel dans les cases à visiter
                        if (isCellDataInToVisit < 0)
                        {
                            AddNewCellData(currentCellNeighboursData, cellsToVisit, use); // ajouter le voisin dans les cases à visiter
                        }
                        else if (currentCellNeighboursData.FCost < cellsToVisit[isCellDataInToVisit].FCost)
                        {
                            cellsToVisit.RemoveAt(isCellDataInToVisit);
                            AddNewCellData(currentCellNeighboursData, cellsToVisit, use); // mettre à jour le voisin dans les cases à visiter
                        }
                    }
                    else if (currentCellNeighboursData.FCost < visitedCells[isCellDataVisited].FCost)
                    {
                        visitedCells.RemoveAt(isCellDataVisited);
                        AddNewCellData(currentCellNeighboursData, visitedCells, use); // mettre à jour le voisin dans les cases visitées
                    }
                    
                }
            }

            iterations++;
        }
        

        if (endCellFound)
        {
            currentCellData = new CellData(currentCellData, finishCell, finishCell);
            while (currentCellData.cell.offsetCoordinates != startCell.offsetCoordinates) // refaire le chemin en sens inverse avec les cases précédentes
            {
                pathCoordinates.Insert(0, currentCellData.cell);
                currentCellData = currentCellData.parentCellData;
            }
            pathCoordinates.Insert(0, startCell);

            return pathCoordinates;
        }
        else
        {
            return null;
        }
    }

    private class CellDataComparer : IComparer<CellData>
    {
        public int Compare(CellData a, CellData b)
        {
            if(a.FCost < b.FCost)
            {
                return -1;
            }
            else if(a.FCost > b.FCost)
            {
                return 1;
            }
            else
            {
                if(a.HCost < b.HCost)
                {
                    return -1;
                }
                if(a.HCost > b.HCost)
                {
                    return 1;
                }
                return 0;
            }
        }
    }

    private void AddNewCellData3(CellData cellData, List<CellData> cellDataList)
    {
        cellDataList.Add(cellData);
        cellDataList.Sort(new CellDataComparer());
    }

    private void AddNewCellData2(CellData cellData,  List<CellData> cellDataList)
    {
        int index = cellDataList.BinarySearch(cellData, new CellDataComparer());

        if (index >= 0)
        {
            cellDataList.Insert(index, cellData);
        }
        else
        {
            index = ~index;

            if (index >= cellDataList.Count)
            {
                cellDataList.Add(cellData);
            }
            else
            {
                cellDataList.Insert(index, cellData);
            }
        }
    }

    // rajoute un élément à une liste triée
    private void AddNewCellData(CellData cellData, List<CellData> cellDataList, int use)
    {
        if (use == 2)
        {
            AddNewCellData2(cellData, cellDataList);
            return;
        }
        if (use == 3)
        {
            AddNewCellData3(cellData, cellDataList);
            return;
        }

        int i = 0;
        while (i < cellDataList.Count && cellDataList[i].FCost < cellData.FCost) // trie par F_cost croissants
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
            while (i < cellDataList.Count && cellDataList[i].FCost == cellData.FCost && cellDataList[i].HCost < cellData.HCost) // trie secondairement par H_cost croissants
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

    // recherche séquentielle d'un élément dans une liste
    private int GetCellDataIndex(CellData cellData, List<CellData> cellDataList)
    {
        for (int i = 0; i < cellDataList.Count; i++)
        {
            if (cellDataList[i].cell.offsetCoordinates == cellData.cell.offsetCoordinates) // cherche la case avec ses coordonnées
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

    // renvoie si la case est traversable par l'unité ou non
    public bool IsCellTraversable(HexCell cell, UnitType unitType, bool isAI)
    {
        if (!cell.isRevealed && !isAI) // si la case n'est pas révélée et que le joueur appelle la fonction
        {
            return true;
        }
        if(unitType.unitCategory == UnitType.UnitCategory.military)
        {
            if(cell.militaryUnit != null && !queuedUnitMovements.ContainsKey(cell.militaryUnit.id)) // si la case n'est pas occupée par une unité statique
            {
                return false;
            }
        }
        else
        {
            if (cell.civilianUnit != null && !queuedUnitMovements.ContainsKey(cell.civilianUnit.id)) // si la case n'est pas occupée par une unité statique
            {
                return false;
            }
        }

        return IsTerrainTypeTraversable(cell.terrainType, unitType);
    }

    // renvoie si le type de terrain est traversable par l'unité ou non
    public bool IsTerrainTypeTraversable(TerrainType terrainType, UnitType unitType)
    {
        if (unitType.speciallyAccessibleTerrains.Contains(terrainType)) // si l'unité peut spécifiquement accéder à ce terrain
        {
            return true;
        }

        if (terrainType.terrainCost > unitType.MoveReach) // si le terrain coute plus en déplacement que ce que dispose l'unité par tour
        {
            return false;
        }
        if ((terrainType.isWater && !unitType.IsABoat) || (!terrainType.isWater && unitType.IsABoat)) // si le terrain est aquatique ou si l'unité est un bateau
        {
            return false;
        }
        return true;
    }

    public List<BenchMarkResult> AStarBenchmark(HexCell startCell, HexCell endCell, float[] heuristicFactorProperties, float[] heuristicScalingProperties, bool printResults)
    {
        List<BenchMarkResult> results = new List<BenchMarkResult>();
        string resultString = "[" + GetDistance(startCell, endCell).ToString().Replace(",", ".");

        for (float i = heuristicFactorProperties[0]; i <= heuristicFactorProperties[1]; i+= (heuristicFactorProperties[1] - heuristicFactorProperties[0]) / heuristicFactorProperties[2])
        {
            for (float j = heuristicScalingProperties[0]; j <= heuristicScalingProperties[1]; j += (heuristicScalingProperties[1] - heuristicScalingProperties[0]) / heuristicScalingProperties[2])
            {
                heuristicFactor = i;
                heuristicScaling = j;

                BenchMarkResult result = new BenchMarkResult();
                result.heuristicFactor = i;
                result.heuristicScaling = j;

                System.DateTime startTime = System.DateTime.Now;

                List<HexCell> path = GetShortestPath(startCell, endCell, GetUnitType("Warrior"), true, 1);

                System.DateTime endTime = System.DateTime.Now;
                string timeElapsed = endTime.Subtract(startTime).TotalMilliseconds.ToString().Replace(",", ".");
                result.timeElapsed = timeElapsed;

                startTime = System.DateTime.Now;

                List<HexCell> path2 = GetShortestPath(startCell, endCell, GetUnitType("Warrior"), true, 2);

                endTime = System.DateTime.Now;
                string timeElapsed2 = endTime.Subtract(startTime).TotalMilliseconds.ToString().Replace(",", ".");
                result.timeElapsed2 = timeElapsed2;

                startTime = System.DateTime.Now;

                List<HexCell> path3 = GetShortestPath(startCell, endCell, GetUnitType("Warrior"), true, 3);

                endTime = System.DateTime.Now;
                string timeElapsed3 = endTime.Subtract(startTime).TotalMilliseconds.ToString().Replace(",", ".");
                result.timeElapsed3 = timeElapsed3;

                float pathCost = 0;
                if (path != null)
                {
                    foreach (var cell in path)
                    {
                        pathCost += cell.terrainType.terrainCost;
                    }
                }
                result.pathCost = pathCost;

                float pathCost2 = 0;
                if (path2 != null)
                {
                    foreach (var cell in path2)
                    {
                        pathCost2 += cell.terrainType.terrainCost;
                    }
                }
                result.pathCost2 = pathCost2;

                float pathCost3 = 0;
                if (path3 != null)
                {
                    foreach (var cell in path3)
                    {
                        pathCost3 += cell.terrainType.terrainCost;
                    }
                }
                result.pathCost3 = pathCost3;

                results.Add(result);

                if (printResults)
                {
                    resultString += ", [" + i + ", " + j + ", " + timeElapsed + ", " + timeElapsed2 + ", " + timeElapsed3 + ", " + pathCost + ", " + pathCost2 + ", " + pathCost3 + "]";
                }
            }
        }

        resultString += "]";
        Debug.Log(resultString);

        return results;
    }

    // classe utile pour stocker les données de chaque case dans le cadre de l'algorithme A*
    private class CellData
    {
        private readonly float GCost; // cout de déplacement brut
        public readonly float HCost; // cout de déplacement heuristique
        public readonly float FCost; // cout de déplacement total
        public readonly HexCell cell; // case
        public readonly CellData parentCellData; // case prédécésseure

        // constructeur de la classe
        public CellData(CellData parentCellData, HexCell cell, HexCell destCell)
        {
            this.GCost = cell.terrainType.terrainCost;
            if (!cell.isRevealed)
            {
                this.GCost = 1000;
            }
            this.HCost = UnitManager.instance.GetDistance(cell, destCell) * UnitManager.instance.heuristicScaling;
            this.FCost = GCost + HCost * UnitManager.instance.heuristicFactor;

            this.cell = cell;
            this.parentCellData = parentCellData;
        }
    }
}

// struct pour enregistrer les éléments du mouvement d'une unité
[Serializable]
public struct queuedMovementData
{
    public int unitToAttackId;
    public bool attacksACity;
    public List<HexCell> path;
    public Action unitAction;
}

public struct FirstMovementData
{
    public bool movementFinished;
    public HexCell unitCell;
}

public struct BenchMarkResult
{
    public float heuristicFactor;
    public float heuristicScaling;

    public string timeElapsed;
    public string timeElapsed2;
    public string timeElapsed3;

    public float pathCost;
    public float pathCost2;
    public float pathCost3;
}