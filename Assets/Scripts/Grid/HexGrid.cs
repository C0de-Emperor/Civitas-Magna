// HexGrid.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [field: SerializeField] public HexOrientation orientation { get; private set; }
    [field: SerializeField] public int width { get; private set; }
    [field: SerializeField] public int height { get; private set; }
    [field: SerializeField] public float hexSize { get; private set; }
    [field: SerializeField] public int batchSize { get; private set; }
    [field: SerializeField] public GameObject undiscoveredTilePrefab { get; private set; }
    [field: SerializeField] public float undiscoveredTileHigh { get; private set; }

    [SerializeField] private Transform raycastTarget;

    [SerializeField] private uint defaultLayer = 1;
    [SerializeField] private uint unactiveLayer = 2;
    [field: SerializeField] public Transform tileContainer { get; private set; }

    [SerializeField] private Dictionary<Vector2, HexCell> cells = new Dictionary<Vector2, HexCell>();

    [SerializeField] private List<HexCell> militaryOccupiedCells = new List<HexCell>();
    [SerializeField] private List<HexCell> supportOccupiedCells = new List<HexCell>();

    public event Action<float> OnCellBatchGenerated;
    public event Action OnCellInstancesGenerated;

    private Vector3 gridOrigin;

    private void Awake()
    {
        raycastTarget.position = new Vector3(-1, 1.3f, -1);
        raycastTarget.localScale = new Vector3(width / 4, 1, height / 4);

        gridOrigin = transform.position;

        OnCellInstancesGenerated += AssignNeighbours;
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

    public void AssignNeighbours()
    {
        foreach (var cell in cells.Values)
        {
            List<HexCell> neighbours = new List<HexCell>();

            // Offsets pour coordonn�es axiales
            Vector2[] axialDirections = new Vector2[]
            {
            new Vector2(1, 0),   // E
            new Vector2(1, -1),  // NE
            new Vector2(0, -1),  // NW
            new Vector2(-1, 0),  // W
            new Vector2(-1, 1),  // SW
            new Vector2(0, 1)    // SE
            };

            foreach (var dir in axialDirections)
            {
                Vector2 neighbourAxial = cell.axialCoordinates + dir;

                Vector3 cube = HexMetrics.AxialToCube(neighbourAxial);
                Vector2 offset = HexMetrics.CubeToOffset(cube, orientation);

                if (cells.TryGetValue(offset, out HexCell neighbour))
                {
                    neighbours.Add(neighbour);
                }
            }

            cell.SetNeighbours(neighbours);
        }

        Debug.Log($"Neighbours assigned for {cells.Count} cells.");

        MapGenerator.instance.isMapReady = true;
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

    /// <summary>
    /// R�v�le toutes les tuiles dans un rayon donn� autour d'une cellule.
    /// </summary>
    /// <param name="centerCellOffsetpositions">Les coordonn�es de la tuile centrale</param>
    /// <param name="radius">Le rayon (en nombre de tuiles hexagonales)</param>
    public void RevealTilesInRadius(Vector2 centerCellOffsetpositions, int radius)
    {
        // Pour �viter les allocations r�currentes
        List<HexCell> toReveal = new List<HexCell>(1 + 3 * radius * (radius + 1));

        Vector3 centerCube = HexMetrics.OffsetToCube(centerCellOffsetpositions, orientation);

        // Parcours dans le cube space (le plus efficace pour les hex)
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = Mathf.Max(-radius, -dx - radius); dy <= Mathf.Min(radius, -dx + radius); dy++)
            {
                int dz = -dx - dy;
                Vector3 neighbourCube = new Vector3(centerCube.x + dx, centerCube.y + dy, centerCube.z + dz);

                // Convertir cube -> offset selon ton orientation
                Vector2 offset = HexMetrics.CubeToOffset(neighbourCube, orientation);

                if (cells.TryGetValue(offset, out HexCell neighbour))
                {
                    toReveal.Add(neighbour);
                }
            }
        }

        // R�v�ler les tuiles trouv�es
        foreach (var cell in toReveal)
        {
            cell.RevealTile();
        }
    }

    public void AddMilitaryUnit(HexCell cell, MilitaryUnit unit)
    {
        if (cell.militaryUnit == null)
        {
            Transform unitTransform = Instantiate(
                unit.Prefab,
                new Vector3(cell.tile.position.x, cell.terrainHigh, cell.tile.position.z),
                new Quaternion(0, 0, 0, 1),
                cell.tile
                );

            cell.militaryUnit = unitTransform;
            militaryOccupiedCells.Add(cell);
        }
        else
        {
            Debug.LogWarning("trying to add a unit on an alreday occupied tile");
        }

        return;
    }

    public void AddSupportUnit(HexCell cell, SupportUnit unit)
    {
        if (cell.supportUnit == null)
        {
            Transform unitTransform = Instantiate(
                unit.Prefab,
                new Vector3(cell.tile.position.x, cell.terrainHigh, cell.tile.position.z),
                new Quaternion(0, 0, 0, 1),
                cell.tile
                );

            cell.supportUnit = unitTransform;
            supportOccupiedCells.Add(cell);
        }
        else
        {
            Debug.LogWarning("trying to add a unit on an alreday occupied tile");
        }

        return;
    public void SetActiveTile(HexCell cell, bool value)
    {
        if(cell.isActive == value) 
            return;

        if (!cell.isRevealed)
        {
            Debug.LogError("On ne peut pas rendre active une tile non d�couverte");
            return;
        }

        var allRenderers = cell.tile.GetComponentsInChildren<Renderer>(true).ToList();

        if (cell.ressource != null)
        {
            allRenderers.AddRange(cell.ressource.GetComponentsInChildren<Renderer>(true));
        }

        // Apply layer mask
        foreach (Renderer rend in allRenderers)
        {
            rend.renderingLayerMask = value ? defaultLayer : unactiveLayer;
        }

        cell.isActive = value;
    }

    public void UpdateActiveTiles()
    {
        // parcourir les tiles
        // si la tile est dans la range d'une troupe / ville / etc ...
            // si elle n'est pas deja active
                // set actie la tile
        // sinon
            // si elle n'est pas deja inactive
                // set inactive
    }
}


public enum HexOrientation
{
    FlatTop,
    PointyTop
}
