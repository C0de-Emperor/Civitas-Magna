using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitManager : MonoBehaviour
{
    [SerializeField] HexGrid grid;
    [SerializeField] GameObject unitPinPrefab;
    [SerializeField] Transform unitPinCanvas;
    [SerializeField] Transform unitContainer;

    public MilitaryUnitType[] militaryUnits;
    public CivilianUnitType[] civilianUnits;

    [HideInInspector] public int nextAvailableId = 1;
    public Dictionary<int, Unit> units { get; private set; }
    public Dictionary<int, queuedMovementData> queuedUnitMovements = new Dictionary<int, queuedMovementData>();

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

        maxIterations = grid.height * grid.width;

        HideActionPanel();
    }


    public void UpdatePinsScale(float newDistance)
    {
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
                Debug.Log(unitData.masterId + " " + PlayerManager.instance.player.id);
                Unit unit = AddUnit(unitData.unitType, grid.GetTile(unitData.cellCoordinates), PlayerManager.instance.playerEntities[unitData.masterId]);
                unit.currentHealth = unitData.currentHealth;
                unit.movesDone = unitData.movesDone;
                unit.lastDamagingTurn = unitData.lastDamagingTurn;
                unit.chargesLeft = unitData.chargesLeft;

                if (unitData.queuedMovementData.path.Count > 1) 
                { 
                    queuedUnitMovements.Add(unit.id, unitData.queuedMovementData); 
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
            actionButton.button.onClick.AddListener(delegate { CivilianUnitAction(building); UpdateActionPanel(cell); });
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

    public void CivilianUnitAction(Building building)
    {
        if(SelectionManager.instance.selectedUnit == null)
        {
            Debug.LogError("trying to execute action with no selected unit ?!");
            return;
        }

        if (SelectionManager.instance.selectedCell.CreateBuilding(building, SelectionManager.instance.selectedUnit))
        {
            if (SelectionManager.instance.selectedUnit.ConsumeCharge())
            {
                RemoveUnit(UnitType.UnitCategory.civilian, SelectionManager.instance.selectedCell);
                CheckCellUnitsConflict(SelectionManager.instance.selectedCell);
                SelectionManager.instance.selectedUnit = null;
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

    public IEnumerator MoveUnit(Queue<HexCell> nextMoves, Unit unit, HexCell cellToAttack)
    {
        HexCell lastCell = nextMoves.Dequeue();
        HexCell nextCell = lastCell;

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

                    grid.RevealTilesInRadius(nextCell.offsetCoordinates, unit.unitType.sightRadius, SelectionManager.instance.showOverlay, true); // révéler les cases "découvertes" par l'unité
                    grid.UpdateActiveTiles(); // mettre à jour les cases découvertes par l'unité
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

                    grid.RevealTilesInRadius(nextCell.offsetCoordinates, unit.unitType.sightRadius, SelectionManager.instance.showOverlay, true); // révéler les cases "découvertes" par l'unité
                    grid.UpdateActiveTiles(); // mettre à jour les cases découvertes par l'unité
                }
                else
                {
                    yield break; // arrête le déplacement pour ce tour
                }

                lastCell = nextCell;
            }
        }
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
        while (path.Count>1 && (unit.movesDone + pathCost + path[1].terrainType.terrainCost <= unit.unitType.MoveReach  || unit.unitType.speciallyAccessibleTerrains.Contains(path[1].terrainType)) && ((movementData.unitToAttackId==-1 && !movementData.attacksACity)|| GetDistance(path[0], path[path.Count-1]) > unit.GetUnitMilitaryData().AttackRange)) // tant que l'unité peut se déplacer et qu'on n'est pas à portée de l'unité à attaquer
        {
            nextMoves.Enqueue(path[1]); // mettre dans la file la case sur laquelle on doit aller
            pathCost += path[1].terrainType.terrainCost;
            path.RemoveAt(0);
        }

        if ((movementData.unitToAttackId != -1 && GetDistance(path[0], path[path.Count - 1]) <= unit.GetUnitMilitaryData().AttackRange) || (movementData.attacksACity == true && GetDistance(path[0], path[path.Count - 1]) <= unit.GetUnitMilitaryData().AttackRange))
        {
            StartCoroutine(MoveUnit(nextMoves, unit, path[path.Count-1])); // faire le déplacement et attaquer l'unité ennemie
        }
        else
        {
            StartCoroutine(MoveUnit(nextMoves, unit, null)); // faire le déplacement
        }
        unit.movesDone += pathCost;
        dataToReturn.unitCell = path[0];

        if (path.Count <= 1)
        {
            dataToReturn.movementFinished = true;
            return dataToReturn; // si on est arrivés à destination, on arrête le déplacement
        }
        dataToReturn.movementFinished = false;
        return dataToReturn;
    }

    // ajouter à la liste d'attente un déplacement d'unité
    public HexCell QueueUnitMovement(HexCell unitCell, HexCell destCell, UnitType.UnitCategory unitCategory)
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

        List<HexCell> path = GetShortestPath(unitCell, destCell, unit.unitType);
        if (path.Count <= 1)
        {
            if (queuedUnitMovements.ContainsKey(unit.id)) // utilisé si le joueur veut retirer l'unité de la liste d'attente
            {
                queuedUnitMovements.Remove(unit.id);
            }
            return destCell;
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

    // ajouter un type d'unité à une case
    public Unit AddUnit(UnitType unitType, HexCell cell, Player master)
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

                if (master == PlayerManager.instance.player)
                {
                    grid.RevealTilesInRadius(cell.offsetCoordinates, unitType.sightRadius, SelectionManager.instance.showOverlay, true);
                }

                if (!cell.isRevealed || !cell.isActive)
                {
                    unitTransform.GetComponentInChildren<Renderer>().enabled = false;
                    unitPin.gameObject.SetActive(false);
                }

                Unit unit = new Unit(unitTransform, unitType, unitPin, master); // créer l'instance de la classe unit associée à l'unité

                cell.militaryUnit = unit;
                units.Add(unit.id, unit);

                CheckCellUnitsConflict(cell);

                return unit;
            }
            else
            {
                Debug.LogError("trying to add a military unit on an already occupied tile");
            }
        }
        else
        {
            if (cell.civilianUnit == null && IsCellTraversable(cell.terrainType, unitType))
            {
                Transform unitTransform = Instantiate( // instancier l'unité sur la case
                    unitType.unitPrefab.transform,
                    new Vector3(cell.tile.position.x, cell.terrainHigh, cell.tile.position.z),
                    new Quaternion(0, 0, 0, 1),
                    unitContainer);
                Transform unitPinTransform = Instantiate(unitPinPrefab.transform, unitPinCanvas); // instancier le pin de l'unité sur l'unité

                UnitPin unitPin = unitPinTransform.GetComponent<UnitPin>();

                if (master == PlayerManager.instance.player)
                {
                    grid.RevealTilesInRadius(cell.offsetCoordinates, unitType.sightRadius, SelectionManager.instance.showOverlay, true);
                }

                if (!cell.isRevealed)
                {
                    unitTransform.GetComponentInChildren<Renderer>().enabled = false;
                    unitPin.GetComponentInChildren<Renderer>().enabled = false;
                }

                Unit unit = new Unit(unitTransform, unitType, unitPin, master); // créer l'instance de la classe unit associée à l'unité

                cell.civilianUnit = unit;
                units.Add(unit.id, unit);

                CheckCellUnitsConflict(cell);

                return unit;
            }
            else
            {
                Debug.LogError("trying to add a support unit on an already occupied tile");
            }
        }
        return null;
    }

    // renvoie une liste de cases qui correspond au chemin le plus court entre deux cases
    public List<HexCell> GetShortestPath(HexCell startCell, HexCell finishCell, UnitType unitType)
    {
        bool endCellFound = false;
        List<HexCell> pathCoordinates = new List<HexCell>();
        CellData startCellData = CreateCellData(null, startCell, finishCell);

        List<CellData> visitedCells = new List<CellData>();
        List<CellData> cellsToVisit = new List<CellData>() { startCellData };

        CellData currentCellData = null;

        if (!IsCellTraversable(finishCell.terrainType, unitType)) { return null; }

        //System.DateTime startTime = System.DateTime.Now;

        int iterations = 0;
        while (!endCellFound && cellsToVisit.Count > 0 && iterations < maxIterations) // tant qu'il reste des cases à visiter et qu'on a pas trouvé la case de fin
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
                else if (IsCellTraversable(currentCellData.cell.neighbours[i].terrainType, unitType) || !currentCellData.cell.neighbours[i].isRevealed) // si la case est traversable non révélée
                {
                    CellData currentCellNeighboursData = CreateCellData(currentCellData, currentCellData.cell.neighbours[i], finishCell);
                    
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
            currentCellData = CreateCellData(currentCellData, finishCell, finishCell);
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
    private CellData CreateCellData(CellData parentCellData, HexCell cell, HexCell destCell)
    {
        float GCost = cell.terrainType.terrainCost;
        if (!cell.isRevealed)
        {
            GCost = 100;
        }
        float HCost = GetDistance(cell, destCell) * heuristicScaling;
        float FCost = GCost + HCost * heuristicFactor;

        return new CellData(GCost, HCost, FCost, cell, parentCellData);
    }

    // renvoie si la case est traversable par l'unité ou non
    public bool IsCellTraversable(TerrainType terrainType, UnitType unitType)
    {
        if (unitType.speciallyAccessibleTerrains.Contains(terrainType))
        {
            return true;
        }

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
    public Vector3 unitOffset = Vector3.zero;
    public readonly UnitType unitType; // le type d'unité (classe générale)
    public Player master; // le maître de l'unité (le joueur ou l'IA)
    public readonly UnitPin unitPin; // le pin de l'unité (pour le repérer sur la carte)


    //public Transform unitCanvaTransform;


    public string unitName; // le nom de l'unité (personnalisable)

    private MilitaryUnitType militaryUnitType; // type de l'unité spécial militaire
    private CivilianUnitType civilianUnitType; // type de l'unité spécial civil
    public float currentHealth = 0; // vie actuelle de l'unité

    public float movesDone = 0; // le nombre de déplacements effectués ce tour
    public int lastDamagingTurn = -1; // le dernier où l'unité a subi des dégats
    public int chargesLeft = 0;

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
        this.unitName = UnitManager.instance.NAMES_LIST[UnityEngine.Random.Range(0, UnitManager.instance.NAMES_LIST.Length - 1)];

        unitPin.InitializePin(this.unitType.unitIcon, this.master.livery);
        unitPin.worldTarget = this.unitTransform;

        if (unitType.unitCategory == UnitType.UnitCategory.military)
        {
            this.militaryUnitType = unitType as MilitaryUnitType;
            this.currentHealth = this.militaryUnitType.MaxHealth;
        }
        else
        {
            this.civilianUnitType = unitType as CivilianUnitType;
            this.chargesLeft = this.civilianUnitType.actionCharges;
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

    public CivilianUnitType GetUnitCivilianData()
    {
        return this.civilianUnitType;
    }

    public bool ConsumeCharge()
    {
        Debug.Log(this.chargesLeft);
        this.chargesLeft -= 1;
        this.movesDone = this.unitType.MoveReach;
        return this.chargesLeft <= 0;
    }

    public void ApplyNewOffset(Vector3 newOffset)
    {
        this.unitTransform.position += newOffset - this.unitOffset;
        this.unitOffset = newOffset;
    }
}

// struct pour enregistrer les éléments du mouvement d'une unité
[Serializable]
public struct queuedMovementData
{
    public int unitToAttackId;
    public bool attacksACity;
    public List<HexCell> path;
}

public struct FirstMovementData
{
    public bool movementFinished;
    public HexCell unitCell;
}