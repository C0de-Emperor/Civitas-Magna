# ALGORITHMES UTILISÉS

## Algorithme heuristique A*
_Assets/Scripts/Units/UnitManager.cs_

### Nécessité

Dans le jeu le joueur peut déplacer ses unités d'un certain nombre de cases prédéfini par tour, mais ça peut devenir répétitif de déplacer des unités sur 10, 15 cases.

On a donc développé un algorithme de recherche du plus court chemin heuristique A*, afin de pouvoir automatiser le déplacement d'une unité sur une distance conséquente.

### Structure de données

Notre carte est une grille d'hexagones, nous avons donc un graphe où chaque case peut accéder à ses 6 cases adjacentes (voisins).

Ce graphe n'est pas orienté mais pondéré par le coût de déplacement vers la case voisine.

Cette structure de graphe est implémentée avec une classe `HexGrid` (_Assets/Scripts/Grid/HexGrid.cs_), correspondant au graphe, et une classe `HexCell` (_Assets/Scripts/Grid/HexCell.cs_), correspondant aux cases.
Chaque case a une propriété `HexCell.neighbours` où sont stockées les cases voisines.

### Algorithme

L'algorithme A* est la méthode `UnitManager.GetShortestPath`. Elle prend en paramètre la case de départ `startCell`, la case d'arrivée `finishCell` et le type d'unité qui se déplace `unitType`.

L'algorithme A* utilise une structure de données stockant le coût physique, heuristique, total de la case et sa case prédécesseure, ainsi que deux listes : une liste de cases déjà visitées `visitedCells`, une liste de cases adjacentes aux cases déjà visitées `cellsToVisit`.

Cette structure de données est la classe `CellData`:

```csharp
private class CellData
{
    private readonly float GCost; // cout de déplacement brut
    public readonly float HCost; // cout de déplacement heuristique
    public readonly float FCost; // cout de déplacement total
    public readonly HexCell cell; // case
    public readonly CellData parentCellData; // case prédécésseure
}
```

L'algorithme A* est le suivant:

* Choisir la `CellData` ayant le coût total le plus faible (en cas d'égalité on choisit le coût heuristique le plus faible)
```csharp
currentCellData = cellsToVisit[0];
```
* Cycler dans chaque voisin de la case choisie :
    * Vérifier que ce voisin existe et qu'il n'est pas la case d'arrivée
    * Si la case est traversable par l'unité (=accessible),  on lui crée son `CellData` et:
        * si elle n'a pas déjà été visitée, on l'ajoute à la liste de case à visiter
        * si elle a déjà été visitée, on met à jour sa valeur dans la liste de cases visitées
```csharp
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
                else if (IsCellTraversable(currentCellData.cell.neighbours[i], unitType)) // si la case est traversable non révélée
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
```
Les fonctions utilisées ici sont les suivantes :

`AddNewCellData` permet d'ajouter une nouvelle `CellData` dans la liste triée donnée en paramètre à la bonne place.
```csharp
// rajoute un élément à une liste triée
private void AddNewCellData(CellData cellData,List<CellData> cellDataList) // A REFAIRE EN DICHOTOMIE
{
    int i = 0;
    while (i < cellDataList.Count && cellDataList[i].FCost< cellData.FCost)
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
        while (i < cellDataList.Count && cellDataList[i]FCost == cellData.FCost && cellDataList[i].HCost <cellData.HCost)
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
```

`GetDistance` renvoie la distance entre deux cases dans la grille (1 -> cases adjacentes, 2 -> une case de distance).
```csharp
// renvoie la distance entre deux cases (en nombre decases, pas distance brute)
public float GetDistance(HexCell cell1, HexCell cell2)
{
    float euclideanDistance = Vector3.Distance(cell1cubeCoordinates, cell2.cubeCoordinates)/grid.hexSize;
    float realDistance = euclideanDistance/Mathf.Sqrt(2);
    return Mathf.Round(realDistance * 100) / 100;
}
```

`IsCellData` renvoie un booléen indiquant si la case est traversable (=accessible) par l'unité (idem pour `IsTerrainTypeTraversable`, mais cette dernière est moins générique).
```csharp
// renvoie si la case est traversable par l'unité ou non
public bool IsCellTraversable(HexCell cell, UnitTypeunitType)
{
    if (!cell.isRevealed)
    {
        return true;
    }
    if(unitType.unitCategory == UnitType.UnitCategorymilitary)
    {
        if(cell.militaryUnit != null &&!queuedUnitMovements.ContainsKey(cell.militaryUnitid))
        {
            return false;
        }
    }
    else
    {
        if (cell.civilianUnit != null &&!queuedUnitMovements.ContainsKey(cell.civilianUnitid))
        {
            return false;
        }
    }
    return IsTerrainTypeTraversable(cell.terrainType,unitType);
}
// renvoie si le type de terrain est traversable parl'unité ou non
public bool IsTerrainTypeTraversable(TerrainTypeterrainType, UnitType unitType)
{
    if (unitType.speciallyAccessibleTerrains.Contain(terrainType))
    {
        return true;
    }
    if (terrainType.terrainCost > unitType.MoveReach)
    {
        return false;
    }
    if ((terrainType.isWater && !unitType.IsABoat) ||(!terrainType.isWater && unitType.IsABoat))
    {
        return false;
    }
    return true;
}
```

Une fois la boucle terminée, on rembobine les prédécesseurs pour trouver le chemin le plus rapide :
```csharp
currentCellData = CreateCellData(currentCellData, finishCell, finishCell);
while (currentCellData.cell.offsetCoordinates startCell.offsetCoordinates) // refaire le chemen sens inverse avec les cases précédentes
{
    pathCoordinates.Insert(0, currentCellDacell);
    currentCellData = currentCellDaparentCellData;
}
pathCoordinates.Insert(0, startCell
System.DateTime endTime = System.DateTime.Now;
//Debug.Log("FOUND PATH FROM " + startCeoffsetCoordinates + " TO " + finishCeoffsetCoordinates + " IN " + endTime.Subtr(startTime) + "s AND " + iterations +iterations");

return pathCoordinates;
```

### Performances
Des tests de vitesse nous donnent qu'en moyenne l'algorithme découvre 60 à 70 cases du chemin optimal par milliseconde, ce qui demande donc généralement moins d'une milliseconde pour calculer tout chemin de moins de 50 cases (compatible avec le fait que le jeu s'actualise tous les 60ièmes de seconde).