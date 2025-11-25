using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[Serializable]
public class HexCell
{
    [Header("Cell Properties")]
    [Header("Terrain")]
    [SerializeField] public HexOrientation orientation;
    [field:SerializeField] public HexGrid grid {  get; set; }
    [field:SerializeField] public float hexSize {  get; set; }
    [field:SerializeField] public TerrainType terrainType {  get; private set; }
    [field:SerializeField] public float terrainHigh {  get; set; }


    [Header("Positions")]
    [field:SerializeField] public Vector2Int offsetCoordinates {  get; set; }
    [field:SerializeField] public Vector3 cubeCoordinates {  get; private set; }
    [field:SerializeField] public Vector2 axialCoordinates {  get; private set; }

    [field: NonSerialized] public HexCell[] neighbours = new HexCell[6];


    [Header("Objects")]
    [field: SerializeField] public Transform tile { get; set; }
    [field: SerializeField] public Transform ressource { get; set; }
    [field: SerializeField] public TileOverlay tileOverlay { get; set; }

    [Header("Units")]

    [field: SerializeField] public Unit militaryUnit = null;
    [field: SerializeField] public Unit civilianUnit = null;


    [Header("Properties")]
    [field: SerializeField] public bool isRevealed { get; set; }
    [field: SerializeField] public bool isActive { get; set; }
    [field: SerializeField] public Building building;

    public void SetCoordinates(Vector2Int _offsetCoordinates, HexOrientation orientation)
    {
        this.orientation = orientation;
        offsetCoordinates = _offsetCoordinates;
        cubeCoordinates = HexMetrics.OffsetToCube(_offsetCoordinates, orientation);
        axialCoordinates = HexMetrics.CubeToAxial(cubeCoordinates);
    }

    public void SetTerrainType(TerrainType _terrainType)
    {
        terrainType = _terrainType;
    }

    public void CreateTerrain()
    {
        if (grid == null || hexSize == 0 || grid.undiscoveredTilePrefab == null)
        {
            Debug.LogError("Missing data for terrain creation");
            return;
        }

        if (isRevealed)
        {
            CreateRevealedTile();

            SetActiveCreatedTile(isActive);
        }
        else
        {
            InstantiateTile(grid.undiscoveredTilePrefab.transform, null, grid.undiscoveredTileHigh);
        }
    }

    // Only for generation !!!!!
    private void CreateRevealedTile()
    {
        if (terrainType == null || grid == null || hexSize == 0 || terrainType.prefab == null)
        {
            Debug.LogError("Missing data for terrain creation");
            return;
        }

        InstantiateTile(terrainType.prefab, terrainType.prop, terrainHigh);
    }

    private void SetActiveCreatedTile (bool value)
    {
        if (!isRevealed)
        {
            Debug.LogError("On ne peut pas rendre active une tile non découverte");
            return;
        }

        var allRenderers = tile.GetComponentsInChildren<Renderer>(true).ToList();

        if (ressource != null)
        {
            allRenderers.AddRange(ressource.GetComponentsInChildren<Renderer>(true));
        }

        // Apply layer mask
        foreach (Renderer rend in allRenderers)
        {
            rend.renderingLayerMask = value ? grid.defaultLayer : grid.unactiveLayer;
        }
    }



    public void RevealExistingTile(bool showOverlay, bool _isActive)
    {
        if (terrainType == null || grid == null || hexSize == 0 || terrainType.prefab == null)
        {
            Debug.LogError("Missing data for terrain creation");
            return;
        }



        if (isRevealed)
        {
            if(_isActive && !isActive)
                SetActiveRevealedTile(true);
            return;
        }
        isRevealed = true;

        UnityEngine.Object.Destroy(tile.gameObject);

        InstantiateTile(terrainType.prefab, terrainType.prop, terrainHigh);

        if (militaryUnit != null)
        {
            militaryUnit.unitTransform.GetComponentInChildren<Renderer>().enabled = true;
            militaryUnit.unitPin.gameObject.SetActive(true);
        }
        if (civilianUnit != null)
        {
            civilianUnit.unitTransform.GetComponentInChildren<Renderer>().enabled = true;
            civilianUnit.unitPin.gameObject.SetActive(true);
        }

        if (showOverlay)
        {
            ShowOverlay();
            if(ressource != null)
                ressource.gameObject.SetActive(false);
        }

        if (!_isActive)
        {
            isActive = false;
            var allRenderers = tile.GetComponentsInChildren<Renderer>(true).ToList();

            if (ressource != null)
            {
                allRenderers.AddRange(ressource.GetComponentsInChildren<Renderer>(true));
            }

            // Apply layer mask
            foreach (Renderer rend in allRenderers)
            {
                rend.renderingLayerMask = grid.unactiveLayer;
            }
        }
    }

    private void InstantiateTile(Transform tilePrefab, Transform ressourcePrefab, float high)
    {
        Vector3 centerPosition = HexMetrics.Center(
            hexSize,
            (int)offsetCoordinates.x,
            (int)offsetCoordinates.y,
            orientation
        ) + grid.transform.position;

        tile = UnityEngine.Object.Instantiate(
            tilePrefab,
            centerPosition,
            Quaternion.identity,
            grid.tileContainer
        );

        Renderer rend = tile.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            Vector3 prefabSize = rend.localBounds.size;

            float targetX, targetZ;

            if (orientation == HexOrientation.FlatTop)
            {
                float idealWidth = HexMetrics.OuterRadius(hexSize) * 2f;
                float idealHeight = HexMetrics.InnerRadius(hexSize) * 2f;

                // Moyenne pour conserver les proportions
                float averageScale = (idealWidth / prefabSize.x + idealHeight / prefabSize.z) / 2f;

                targetX = prefabSize.x * averageScale;
                targetZ = prefabSize.z * averageScale;
            }
            else // PointyTop
            {
                targetX = HexMetrics.InnerRadius(hexSize) * 2f;
                targetZ = HexMetrics.OuterRadius(hexSize) * 2f;
            }

            Vector3 newScale = tile.transform.localScale;
            newScale.x = targetX / prefabSize.x * newScale.x;
            newScale.y = high;
            newScale.z = targetZ / prefabSize.z * newScale.z;
            tile.transform.localScale = newScale;
        }

        tile.gameObject.layer = LayerMask.NameToLayer("Grid");
        tile.gameObject.isStatic = true;

        if (orientation == HexOrientation.FlatTop)
        {
            tile.Rotate(new Vector3(0, 30, 0));
        }

        int randomRotation = UnityEngine.Random.Range(0, 6);
        tile.Rotate(new Vector3(0, randomRotation * 60, 0));

        if (building.buildingName != Building.BuildingNames.None && building.buildingName != Building.BuildingNames.City)
        {
            InstantiateRessource(building.buildingPrefab.transform);
        }
        else if (ressourcePrefab != null)
        {
            InstantiateRessource(ressourcePrefab);
        }
    }

    public Transform InstantiateRessource(Transform ressourcePrefab)
    {
        if(ressource != null)
        {
            grid.DestroyRessource(this);
        }

        Vector3 centerPosition = HexMetrics.Center(
            hexSize,
            offsetCoordinates.x,
            offsetCoordinates.y,
            orientation
        ) + grid.transform.position;

        ressource = UnityEngine.Object.Instantiate(
            ressourcePrefab,
            centerPosition + new Vector3(0, terrainHigh, 0),
            Quaternion.identity,
            grid.tileContainer
        );

        Renderer rend = tile.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            Vector3 prefabSize = rend.localBounds.size;

            float targetX, targetZ;

            if (orientation == HexOrientation.FlatTop)
            {
                float idealWidth = HexMetrics.OuterRadius(hexSize) * 2f;
                float idealHeight = HexMetrics.InnerRadius(hexSize) * 2f;

                // Moyenne pour conserver les proportions
                float averageScale = (idealWidth / prefabSize.x + idealHeight / prefabSize.z) / 2f;

                targetX = prefabSize.x * averageScale;
                targetZ = prefabSize.z * averageScale;
            }
            else // PointyTop
            {
                targetX = HexMetrics.InnerRadius(hexSize) * 2f;
                targetZ = HexMetrics.OuterRadius(hexSize) * 2f;
            }

            Vector3 newScale = tile.transform.localScale;
            newScale.x = targetX / prefabSize.x * newScale.x;
            newScale.y = ressourcePrefab.localScale.y * hexSize;
            newScale.z = targetZ / prefabSize.z * newScale.z;
            ressource.transform.localScale = newScale;
        }

        ressource.gameObject.layer = LayerMask.NameToLayer("Grid");
        ressource.gameObject.isStatic = true;
        if (orientation == HexOrientation.FlatTop)
        {
            ressource.Rotate(new Vector3(0, 30, 0));
        }
        int randomRotation = UnityEngine.Random.Range(0, 6);
        ressource.Rotate(new Vector3(0, randomRotation * 60, 0));

        return ressource.transform;
    }

    public void SetNeighbours(HexCell[] _neighbours)
    {
        neighbours = _neighbours;
    }

    public void ClearTerrain()
    {
        if (tile != null)
        {
            UnityEngine.Object.Destroy(tile.gameObject);
        }
    }
  
    public void SetupOverlay(TileOverlay overlay)
    {
        tileOverlay = overlay;
        tileOverlay.gameObject.SetActive(false);
    }

    public void ShowOverlay()
    {
        if (ressource != null && building.buildingName != Building.BuildingNames.City)
            ressource.gameObject.SetActive(false);

        if (ressource != null && building.buildingName == Building.BuildingNames.City)
            ressource.gameObject.GetComponent<City>().HideForOverlay();

        tileOverlay.Init(terrainType.food, terrainType.production);
        tileOverlay.gameObject.SetActive(true);
    }

    public void HideOverlay()
    {
        if (ressource != null)
            ressource.gameObject.SetActive(true);

        if (building.buildingName == Building.BuildingNames.City)
            ressource.gameObject.GetComponent<City>().ShowForOverlay();

        tileOverlay.gameObject.SetActive(false);
    }

    public void SetActiveRevealedTile(bool value)
    {
        if (isActive == value)
            return;

        if (!isRevealed)
        {
            Debug.LogError("On ne peut pas rendre active une tile non découverte");
            return;
        }

        var allRenderers = tile.GetComponentsInChildren<Renderer>(true).ToList();

        if (ressource != null)
        {
            allRenderers.AddRange(ressource.GetComponentsInChildren<Renderer>(true));
        }

        // Apply layer mask
        foreach (Renderer rend in allRenderers)
        {
            rend.renderingLayerMask = value ? grid.defaultLayer : grid.unactiveLayer;
        }


        if (militaryUnit != null)
        {
            militaryUnit.unitTransform.GetComponentInChildren<Renderer>().enabled = value;
            militaryUnit.unitPin.gameObject.SetActive(value);
        }
        if (civilianUnit != null)
        {
            civilianUnit.unitTransform.GetComponentInChildren<Renderer>().enabled = value;
            civilianUnit.unitPin.gameObject.SetActive(value);
        }

        isActive = value;
    }

    public bool CreateBuilding(Building _building, Unit builder)
    {
        if (_building.CanBeBuildOn(this))
        {
            if (_building.buildingName == Building.BuildingNames.City)
            {
                CityManager.instance.CreateCity(this, builder.master);
            }
            else if (_building.buildingPrefab != null)
            {
                InstantiateRessource(_building.buildingPrefab.transform);
                if (SelectionManager.instance.showOverlay)
                {
                    ShowOverlay();
                }
                else
                {
                    HideOverlay();
                }
            }
            building = _building;
            UnitManager.instance.UpdateActionPanel(this);

            return true;
        }
        else
        {
            return false;
        }
    }
}
 