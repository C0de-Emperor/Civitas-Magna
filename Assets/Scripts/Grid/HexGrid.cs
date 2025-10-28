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
    [field: SerializeField] public Transform tileContainer { get; private set; }

    [field: SerializeField] private Transform raycastTarget;

    [field: SerializeField] private Transform overlayParent;

    [field: SerializeField] private TileOverlay overlayPrefab;

    public event Action<float> OnCellBatchGenerated;
    public event Action OnCellInstancesGenerated;

    private Dictionary<Vector2, HexCell> cells = new Dictionary<Vector2, HexCell>();
    private Vector3 gridOrigin;
    private uint defaultLayer = 1;
    private uint unactiveLayer = 2;

    private void Awake()
    {
        raycastTarget.gameObject.SetActive(true);
        raycastTarget.position = new Vector3(-1, 1.3f, -1);
        raycastTarget.localScale = new Vector3(width / 4, 1, height / 4);

        gridOrigin = transform.position;

        OnCellInstancesGenerated += AssignNeighbours;
        OnCellInstancesGenerated += SetTileOverlays;
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
            HexCell[] neighbours = new HexCell[6];

            // Offsets pour coordonn�es axiales
            Vector2[] axialDirections = new Vector2[]
            {
            new Vector2(1, 0),
            new Vector2(1, -1), 
            new Vector2(0, -1),  
            new Vector2(-1, 0), 
            new Vector2(-1, 1), 
            new Vector2(0, 1)
            };

            int i = 0;
            foreach (var dir in axialDirections)
            {
                Vector2 neighbourAxial = cell.axialCoordinates + dir;

                Vector3 cube = HexMetrics.AxialToCube(neighbourAxial);
                Vector2 offset = HexMetrics.CubeToOffset(cube, orientation);

                if (cells.TryGetValue(offset, out HexCell neighbour))
                {
                    neighbours[i]=neighbour;
                }
                else
                {
                    neighbours[i]=null;
                }
                i++;
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
    /// Révèle toutes les tuiles dans un rayon donn autour d'une cellule.
    /// </summary>
    /// <param name="centerCellOffsetpositions">Les coordonnées de la tuile centrale</param>
    /// <param name="radius">Le rayon (en nombre de tuiles hexagonales)</param>
    public void RevealTilesInRadius(Vector2 centerCellOffsetpositions, int radius, bool showOverlay)
    {
        List<HexCell> toReveal = new List<HexCell>(1 + 3 * radius * (radius + 1));

        Vector3 centerCube = HexMetrics.OffsetToCube(centerCellOffsetpositions, orientation);

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

        // Révéler les tuiles trouvées
        foreach (var cell in toReveal)
        {
            cell.RevealTile(showOverlay);
        }
    }
    
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

    public void SetTileOverlays()
    {
        foreach (HexCell cell in cells.Values)
        {
            TileOverlay overlay = Instantiate(overlayPrefab, overlayParent);
            overlay.transform.localPosition = new Vector3(
                cell.tile.transform.position.x, 
                cell.terrainHigh + 0.001f,
                cell.tile.transform.position.z);
            overlay.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // face vers le haut, par ex.

            cell.SetupOverlay(overlay);
        }
    }

    public void ShowAllOverlay()
    {
        foreach (HexCell cell in cells.Values)
        {
            if(cell.isRevealed && cell.isActive)
                cell.ShowOverlay();
        }
    }

    public void HideAllOverlay()
    {
        foreach (HexCell cell in cells.Values)
        {
            cell.HideOverlay();
        }
    }
}


public enum HexOrientation
{
    FlatTop,
    PointyTop
}
