// HexGrid.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [field: SerializeField] public HexOrientation orientation { get; private set; }
    [field: SerializeField] public int width { get; private set; }
    [field: SerializeField] public int height { get; private set; }
    [field: SerializeField] public float hexSize { get; private set; }
    [field: SerializeField] public int batchSize { get; private set; }

    [SerializeField] private Dictionary<Vector2, HexCell> cells = new Dictionary<Vector2, HexCell>();

    public event Action<float> OnCellBatchGenerated;
    public event Action OnCellInstancesGenerated;

    private Vector3 gridOrigin;

    private void Awake()
    {
        gridOrigin = transform.position;
    }

    public void SetHexCells(List<HexCell> newCells)
    {
        cells.Clear();

        foreach (var cell in newCells)
        {
            cells[cell.offsetCoordinates] = cell;
        }

        StartCoroutine(InstantiateCells());
    }

    private IEnumerator InstantiateCells()
    {
        int batchCount = 0;
        int totalBatches = Mathf.CeilToInt((float)cells.Count / batchSize);

        foreach (var cell in cells.Values)
        {
            cell.grid = this;
            cell.CreateTerrain(); // Instancie le prefab et le positionne

            if (batchCount % batchSize == 0 && batchCount != 0)
            {
                OnCellBatchGenerated?.Invoke((float)batchCount / totalBatches);
                yield return null;
            }

            batchCount++;
        }

        OnCellInstancesGenerated?.Invoke();
    }

    public HexCell GetTile(Vector2 coordinates)
    {
        if (cells.TryGetValue(coordinates, out HexCell cell))
            return cell;

        return null;
    }

    public bool HasTile(Vector2 coordinates)
    {
        return cells.ContainsKey(coordinates);
    }
}

public enum HexOrientation
{
    FlatTop,
    PointyTop
}
