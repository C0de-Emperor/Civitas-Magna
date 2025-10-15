using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField]
    const float HEURISTIC_SCALING = 5.0f;
    const int MAX_ITERATIONS = 1_000_000;

    [Header("Units")]
    public List<Unit> MilitaryUnits = new List<Unit>();
    public List<Unit> SupportUnits = new List<Unit>();

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

    public List<Vector2> GetShortestPath(HexGrid grid, HexCell startCell, HexCell finishCell, float heuristicFactor)
    {
        Debug.LogWarning("SEARCHING PATH FROM "+startCell.offsetCoordinates + " TO "+finishCell.offsetCoordinates);

        bool endCellFound = false;
        List<Vector2> pathCoordinates = new List<Vector2>();
        CellData startCellData = CreateCellData(null, startCell, finishCell, heuristicFactor, grid.hexSize);

        List<CellData> visitedCells = new List<CellData>();
        List<CellData> cellsToVisit = new List<CellData>() { startCellData };

        CellData currentCellData=null;

        int iterations = 0;
        System.DateTime startTime = System.DateTime.Now;
        while (!endCellFound && cellsToVisit.Count > 0 && iterations < MAX_ITERATIONS)
        {
            currentCellData = cellsToVisit[0];
            AddNewCellData(currentCellData, visitedCells);
            cellsToVisit.RemoveAt(0);
            //Debug.LogWarning("looking at cell : " + currentCellData.GetCellDataInfo());

            for (int i = 0; i < currentCellData.cell.neighbours.Count(); i++)
            {
                if (currentCellData.cell.neighbours[i].offsetCoordinates == finishCell.offsetCoordinates)
                {
                    endCellFound = true;
                    break;
                }
                else if (currentCellData.cell.neighbours[i].traversable)
                {
                    CellData currentCellNeighboursData = CreateCellData(currentCellData, currentCellData.cell.neighbours[i], finishCell, heuristicFactor, grid.hexSize);
                    //Debug.Log("looking at neighbour cell : " + currentCellNeighboursData.GetCellDataInfo());
                    int isCellDataVisited = GetCellDataIndex(currentCellNeighboursData, visitedCells);
                    if (isCellDataVisited==-1)
                    {
                        int isCellDataInToVisit = GetCellDataIndex(currentCellNeighboursData, cellsToVisit);
                        if (isCellDataInToVisit==-1)
                        {
                            AddNewCellData(currentCellNeighboursData, cellsToVisit);
                        }
                        else if (currentCellNeighboursData.FCost < cellsToVisit[isCellDataInToVisit].FCost )
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
        Debug.Log("time taken : " + endTime.Subtract(startTime));

        if (endCellFound)
        {
            while (currentCellData.cell.offsetCoordinates != startCell.offsetCoordinates)
            {
                pathCoordinates.Add(currentCellData.cell.offsetCoordinates);
                currentCellData = currentCellData.parentCellData;
            }
            return pathCoordinates;
        }
        else
        {
            return null;
        }
    }

    private void AddNewCellData(CellData cellData,  List<CellData> cellDataList)
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
        else if(cellData.FCost == cellDataList[i].FCost){
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
        for(int i = 0; i < cellDataList.Count; i++)
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
        float xDiff = Mathf.Pow(cell1.axialCoordinates.x - cell2.axialCoordinates.x, 2);
        float yDiff = Mathf.Pow(cell1.axialCoordinates.x - cell2.axialCoordinates.y, 2);
        float euclideanDistance = Mathf.Sqrt(xDiff + yDiff) / hexSize;
        return Mathf.Round(euclideanDistance * 10) / 10;
    }

    private CellData CreateCellData(CellData parentCellData, HexCell cell, HexCell destCell, float heuristicFactor, float hexSize)
    {
        float GCost = GetEuclideanDistance(cell, destCell, hexSize);
        float HCost = cell.terrainCost * HEURISTIC_SCALING;
        float FCost = GCost + HCost * heuristicFactor;
        return new CellData(GCost, HCost, FCost, cell, parentCellData);
    }
}

public class CellData
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

/**
public List<Vector2> GetShortestPath(HexGrid grid, HexCell startCell, HexCell finishCell, float heuristicFactor)
{
    // A* type algorithm
    bool endCellFound = false;
    bool firstIteration = true;
    List<Vector2> pathCoordinates = new List<Vector2>();
    CellData startCellData = CreateCellData(null, startCell, finishCell, heuristicFactor, grid.hexSize);

    TreeNode visitedCellsRoot = new TreeNode(startCellData);
    TreeNode cellsToVisitRoot = new TreeNode(startCellData);

    TreeNode currentTreeNode = visitedCellsRoot;
    CellData currentCellData = startCellData;

    while (!endCellFound && (!cellsToVisitRoot.IsALeaf() || firstIteration))
    {
        if (currentCellData.cell.offsetCoordinates == finishCell.offsetCoordinates) {
            while (currentCellData.cell.offsetCoordinates != startCell.offsetCoordinates)
            {
                pathCoordinates.Add(currentCellData.cell.offsetCoordinates);
                currentCellData = currentCellData.parentCellData;
            }
            endCellFound = true;
        }
        else
        {
            for (int i = 0; i < currentCellData.cell.neighbours.Count(); i++)
            {
                Debug.Log("looking at neighbour cell : "+currentCellData.cell.neighbours[i].offsetCoordinates);
                if (currentCellData.cell.neighbours[i].traversable)
                {
                    CellData currentCellNeighboursData = CreateCellData(currentCellData, currentCellData.cell.neighbours[i], finishCell, heuristicFactor, grid.hexSize);
                    LeafFound isCellDataVisited=visitedCellsRoot.GetCellDataNode(currentCellNeighboursData);
                    if (!isCellDataVisited.cellDataFound)
                    {
                        LeafFound isCellDataInAccessible = cellsToVisitRoot.GetCellDataNode(currentCellNeighboursData);
                        if (!isCellDataInAccessible.cellDataFound)
                        {
                            isCellDataInAccessible.parentNode.AddNewNode(currentCellNeighboursData);
                        }
                    }
                }
            }


        }

        currentTreeNode = cellsToVisitRoot.GetSmallestNodeParent();
        currentCellData = currentTreeNode.GetSmallestCellData(currentTreeNode.value);

        firstIteration = false;
    }


    Debug.Log("fin du programme");
    return pathCoordinates;
}


public class TreeNode
{
public readonly float value;
private List<CellData> cellDatas = new List<CellData>();
private TreeNode leftNode;
private TreeNode rightNode;

public TreeNode(CellData cellData)
{
    this.value = cellData.HCost;
    Debug.Log("creating new node : "+value+" "+cellData.cell.offsetCoordinates);
    this.cellDatas.Add(cellData);
}

public int GetCellDatasCount()
{
    return this.cellDatas.Count;
}

public bool IsALeaf()
{
    return this.leftNode == null && this.rightNode == null;
}

public CellData GetLowestHCostCellData()
{
    CellData lowestHCostCell = this.cellDatas[0];
    foreach (CellData cellData in this.cellDatas)
    {
        if (cellData.HCost < lowestHCostCell.HCost)
        {
            lowestHCostCell = cellData;
        }
    }
    this.cellDatas.Remove(lowestHCostCell);

    return lowestHCostCell;
}

public CellData GetSmallestCellData(float cellDataValue)
{
    CellData smallestCellData;
    if(cellDataValue < this.value)
    {
        smallestCellData=this.leftNode.GetLowestHCostCellData();
        if(this.leftNode.IsALeaf() && this.leftNode.cellDatas.Count == 0)
        {
            Debug.Log("destroying value " + this.leftNode.value);
            this.leftNode = null;
        }
    }
    else if(cellDataValue > this.value)
    {
        smallestCellData=this.rightNode.GetLowestHCostCellData();
        if (this.rightNode.IsALeaf() && this.rightNode.cellDatas.Count == 0)
        {
            Debug.Log("destroying value " + this.rightNode.value);
            this.rightNode = null;
        }
    }
    else
    {
        smallestCellData = this.GetLowestHCostCellData();
    }

    return smallestCellData;
}

public TreeNode GetSmallestNodeParent()
{
    Debug.Log("searching for the smallest node " +this.value);
    if (this.leftNode != null)
    {
        if (this.leftNode.IsALeaf())
        {
            return this;
        }
        return this.leftNode;
    }
    else if(this.rightNode!= null)
    {
        if (this.rightNode.IsALeaf())
        {
            return this;
        }
        return this.rightNode;
    }
    else
    {
        Debug.LogError("too deep in the tree");
        return null;
    }
}

public LeafFound GetCellDataNode(CellData cellData)
{
    if (cellData.FCost < this.value)
    {
        if (this.leftNode == null)
        {
            return new LeafFound(false, this);
        }
        return this.leftNode.GetCellDataNode(cellData);
    }
    else if (cellData.FCost > this.value)
    {
        if (this.rightNode == null)
        {
            return new LeafFound(false, this);
        }
        return this.rightNode.GetCellDataNode(cellData);
    }
    foreach (var item in this.cellDatas)
    {
        if (item.cell.offsetCoordinates == cellData.cell.offsetCoordinates)
        {
            return new LeafFound(true, this);
        }
    }
    return new LeafFound(false, this);
}

public void AddNewNode(CellData cellData)
{
    if (cellData.FCost < this.value)
    {
        if (this.leftNode == null)
        {
            this.leftNode = new TreeNode(cellData);
        }
        else
        {
            Debug.LogWarning("trying to add a leaf to a complete node");
        }
    }
    else if (cellData.FCost > this.value)
    {
        if (this.rightNode == null)
        {
            this.rightNode = new TreeNode(cellData);
        }
        else
        {
            Debug.LogWarning("trying to add a leaf to a complete node");
        }
    }
    else
    {
        this.cellDatas.Add(cellData);
    }
}
}

public struct LeafFound
{
public readonly bool cellDataFound;
public readonly TreeNode parentNode;

public LeafFound(bool cellDataFound, TreeNode parentNode)
{
    this.cellDataFound = cellDataFound;
    this.parentNode = parentNode;
}
}
**/