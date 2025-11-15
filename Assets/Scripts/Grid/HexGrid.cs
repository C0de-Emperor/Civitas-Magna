using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] public Building NoneBuilding;

    [field: SerializeField] private Transform raycastTarget;

    [field: SerializeField] private Transform overlayParent;

    [field: SerializeField] private TileOverlay overlayPrefab;

    public event Action<float> OnCellBatchGenerated;
    public event Action OnCellInstancesGenerated;

    private Dictionary<Vector2, HexCell> cells = new Dictionary<Vector2, HexCell>();
    private Vector3 gridOrigin;
    public readonly uint defaultLayer = 1;
    public readonly uint unactiveLayer = 2;

    public Slider progressBar;
    public Text loading;

    private int generationProgress;

    private void Awake()
    {
        raycastTarget.gameObject.SetActive(true);
        raycastTarget.position = new Vector3(-1, 1.3f, -1);
        raycastTarget.localScale = new Vector3(width / 4, 1, height / 4);

        gridOrigin = transform.position;

        OnCellInstancesGenerated += () => StartCoroutine(AssignNeighbours());
        OnCellInstancesGenerated += () => StartCoroutine(SetTileOverlays());
    }

    private void Start()
    {
        TurnManager.instance.OnTurnChange += UpdateActiveTiles;
    }

    public IEnumerator SetHexCells(List<HexCell> newCells)
    {
        generationProgress = 0;
        progressBar.gameObject.SetActive(true);
        progressBar.maxValue = width * height * 4;
        loading.gameObject.SetActive(true);
        cells.Clear();

        foreach (var cell in newCells)
        {
            cells[cell.offsetCoordinates] = cell;
            generationProgress++;
            progressBar.value = generationProgress;

            if (generationProgress % 200 == 0)
                yield return null;
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
            generationProgress++;
            progressBar.value = generationProgress;
            batchCount++;
        }

        OnCellInstancesGenerated?.Invoke();
    }

    public IEnumerator AssignNeighbours()
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
            generationProgress++;
            progressBar.value = generationProgress;

            // Donne une frame à Unity pour rafraîchir l’UI
            if (generationProgress % 200 == 0)
                yield return null;
        }

        progressBar.gameObject.SetActive(false);
        loading.gameObject.SetActive(false);

        UnityEngine.Debug.Log($"Neighbours assigned for {cells.Count} cells.");

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
    public void RevealTilesInRadius(Vector2 centerCellOffsetCoordinates, int radius, bool showOverlay)
    {
        List<HexCell> toReveal = new List<HexCell>(1 + 3 * radius * (radius + 1));

        Vector3 centerCube = HexMetrics.OffsetToCube(centerCellOffsetCoordinates, orientation);

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
                    neighbour.RevealTile(showOverlay);
                }
            }
        }
    }

    public void SetActiveInRadius(Vector2 centerCellOffsetCoordinates, int radius, bool activate)
    {
        List<HexCell> toActivate = new List<HexCell>(1 + 3 * radius * (radius + 1));

        Vector3 centerCube = HexMetrics.OffsetToCube(centerCellOffsetCoordinates, orientation);

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
                    neighbour.SetActiveTile(activate);
                }
            }
        }
    }

    public void UpdateActiveTiles()
    {
        List<SightData> sightDatas = new List<SightData>();

        foreach (var cell in cells.Values)
        {
            if (cell.isRevealed)
            {
                cell.SetActiveTile(false);
            }

            if(cell.militaryUnit != null && cell.militaryUnit.master == PlayerManager.instance.player)
            {
                SightData unitSightData;
                unitSightData.sightRadius = cell.militaryUnit.unitType.sightRadius;
                unitSightData.cellCoordinates = cell.offsetCoordinates;
                sightDatas.Add(unitSightData);
            }
            if (cell.civilianUnit != null && cell.civilianUnit.master == PlayerManager.instance.player)
            {
                SightData unitSightData;
                unitSightData.sightRadius = cell.civilianUnit.unitType.sightRadius;
                unitSightData.cellCoordinates = cell.offsetCoordinates;
                sightDatas.Add(unitSightData);
            }
        }

        foreach(var city in CityManager.instance.cities.Values)
        {
            foreach(var cellCoordinates in city.controlledTiles.Keys)
            {
                SightData unitSightData;
                unitSightData.sightRadius = 2;
                unitSightData.cellCoordinates = cellCoordinates;
                sightDatas.Add(unitSightData);
            }
        }

        foreach(var sightData in sightDatas)
        {
            SetActiveInRadius(sightData.cellCoordinates, sightData.sightRadius, true);
        }
    }

    public IEnumerator SetTileOverlays()
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
            generationProgress++;
            progressBar.value = generationProgress;

            // Donne une frame à Unity pour rafraîchir l’UI
            if (generationProgress % 200 == 0)
                yield return null;
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

    public HexCellData[] GetAllCellData()
    {
        HexCellData[] cellsData = new HexCellData[cells.Count];

        int i = 0;

        foreach (HexCell cell in cells.Values)
        {
            cellsData[i] = new HexCellData
            {
                terrainType = cell.terrainType,
                terrainHigh = cell.terrainHigh,
                offsetCoordinates = cell.offsetCoordinates,

                isRevealed = cell.isRevealed,
                isActive = cell.isActive,
                buildingName = cell.building.buildingName
            };
            i++;
        }

        return cellsData;
    }

    public void DestroyRessource(HexCell cell)
    {
        Destroy(cell.ressource.gameObject);
    }
}

public enum HexOrientation
{
    FlatTop,
    PointyTop
}

public struct SightData
{
    public Vector2 cellCoordinates;
    public int sightRadius;
}