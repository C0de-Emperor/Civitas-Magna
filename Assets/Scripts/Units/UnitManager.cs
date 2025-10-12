using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Unity.Cinemachine.CinemachineSplineRoll;

public class UnitManager : MonoBehaviour
{
    [Header("Units")]
    public List<Unit> MilitaryUnits = new List<Unit>();
    public List<Unit> SupportUnits = new List<Unit>();

    public List<Vector2> GetShortestPath(HexGrid grid, HexCell startCell, HexCell finishCell, float heuristicFactor)
    {
        // A* type algorithm
        bool endCellFound = false;
        List<Vector2> pathCoordinates = new List<Vector2>();
        CellData startCellData = CreateCellData(null, startCell, finishCell, heuristicFactor);

        TreeNode visitedCellsRoot = new TreeNode(startCellData);
        TreeNode accessibleCellsRoot = new TreeNode(startCellData);

        TreeNode currentTreeNode;
        CellData currentCellData;

        while (!endCellFound)
        {
            currentTreeNode = accessibleCellsRoot.GetSmallestNode();
            currentCellData = currentTreeNode.GetClosestCellData();

            if (currentCellData.cell.axialCoordinates == finishCell.axialCoordinates) {
                while (currentCellData.cell.axialCoordinates != startCell.axialCoordinates)
                {
                    pathCoordinates.Add(currentCellData.cell.axialCoordinates);
                    currentCellData = currentCellData.parentCellData;
                }
                endCellFound = true;
            }
            else
            {
                for (int i = 0; i < currentCellData.cell.neighbours.Count(); i++)
                {
                    Debug.Log(i);
                    if (currentCellData.cell.neighbours[i].traversable)
                    {
                        CellData currentCellNeighboursData = CreateCellData(currentCellData, currentCellData.cell.neighbours[i], finishCell, heuristicFactor);
                        if (visitedCellsRoot.GetCellDataLeaf(currentCellNeighboursData)==null)
                        {
                            TreeNode parentNode = accessibleCellsRoot.GetCellDataLeaf(currentCellNeighboursData);
                            if (parentNode != null)
                            {
                                parentNode.AddNewNode(currentCellNeighboursData);
                            }
                        }
                    }
                }


            }
        }

        return pathCoordinates;
    }
   

    private float GetEuclideanDistance(HexCell cell1, HexCell cell2)
    {
        float xDiff=Mathf.Pow(cell1.tile.position.x - cell2.tile.position.x, 2);
        float yDiff=Mathf.Pow(cell1.tile.position.x - cell2.tile.position.y, 2);
        return Mathf.Sqrt(xDiff + yDiff);
    }

    private CellData CreateCellData(CellData parentCellData, HexCell cell, HexCell destCell, float heuristicFactor)
    {
        float GCost = cell.terrainCost;
        float HCost = GetEuclideanDistance(cell, destCell);
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
}

public class TreeNode
{
    public readonly float value;
    private List<CellData> cellDatas;
    private TreeNode leftNode;
    private TreeNode rightNode;

    public TreeNode(CellData cellData)
    {
        this.value = cellData.HCost;
        Debug.Log(cellData.cell.axialCoordinates);
        this.cellDatas.Add(cellData);
    }

    public int GetCellDatasCount()
    {
        return this.cellDatas.Count;
    }

    public CellData GetClosestCellData()
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

    public TreeNode GetSmallestNode()
    {
        if (this.leftNode == null || this.leftNode.GetCellDatasCount() == 0)
        {
            return this;
        }
        return this.leftNode.GetSmallestNode();
    }

    public TreeNode GetCellDataLeaf(CellData cellData) // returns null if the cellData is in the tree, its leaf if not
    {
        if (cellData.FCost < this.value)
        {
            if (this.leftNode == null)
            {
                return this;
            }
            return this.leftNode.GetCellDataLeaf(cellData);
        }
        else if (cellData.FCost > this.value)
        {
            if (this.rightNode == null)
            {
                return this;
            }
            return this.rightNode.GetCellDataLeaf(cellData);
        }
        foreach (var item in this.cellDatas)
        {
            if (item.cell.axialCoordinates == cellData.cell.axialCoordinates)
            {
                return null;
            }
        }
        return this;
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