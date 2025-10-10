using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HexCell
{
    [Header("Cell Properties")]
    [Header("Terrain")]
    [SerializeField] private HexOrientation orientation;
    [field:SerializeField] public HexGrid grid {  get; set; }
    [field:SerializeField] public float hexSize {  get; set; }
    [field:SerializeField] public TerrainType terrainType {  get; private set; }
    [field:SerializeField] public float terrainHigh {  get; set; }

    [Header("Positions")]
    [field:SerializeField] public Vector2 offsetCoordinates {  get; set; }
    [field:SerializeField] public Vector3 cubeCoordinates {  get; private set; }
    [field:SerializeField] public Vector2 axialCoordinates {  get; private set; }
    [field: NonSerialized] public List<HexCell> neighbours { get; private set; }

    [Header("Objects")]
    [field: SerializeField] public Transform tile { get; set; }
    [field: SerializeField] public Transform ressource { get; set; }

    [Header("Units")]
    [field: SerializeField] public Transform militaryUnit { get; set; }
    [field: SerializeField] public Transform supportUnit { get; set; }

    public void SetCoordinates(Vector2 _offsetCoordinates, HexOrientation orientation)
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
        if (terrainType == null || grid == null || hexSize == 0 || terrainType.prefab == null)
        {
            Debug.LogError("Missing data for terrain creation");
            return;
        }

        Vector3 centrePosition = HexMetrics.Center(
            hexSize,
            (int)offsetCoordinates.x,
            (int)offsetCoordinates.y,
            orientation
        ) + grid.transform.position;

        tile = UnityEngine.Object.Instantiate(
            terrainType.prefab,
            centrePosition,
            Quaternion.identity,
            grid.transform
        );

        if(terrainType.prop != null)
        {
            ressource = UnityEngine.Object.Instantiate(
                terrainType.prop,
                centrePosition + new Vector3(0, terrainHigh, 0),
                Quaternion.identity,
                grid.transform
            );
        }


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
            newScale.y = terrainHigh;
            newScale.z = targetZ / prefabSize.z * newScale.z;
            tile.transform.localScale = newScale;
            if (ressource != null)
            {
                ressource.transform.localScale = new Vector3(newScale.x, ressource.transform.localScale.y, newScale.z);
            }
        }

        tile.gameObject.layer = LayerMask.NameToLayer("Grid");
        tile.gameObject.isStatic = true;
        
        if (orientation == HexOrientation.FlatTop)
        {
            tile.Rotate(new Vector3(0, 30, 0));
        }

        int randomRotation = UnityEngine.Random.Range(0, 6);
        tile.Rotate(new Vector3(0, randomRotation * 60, 0));
        
        if (ressource != null)
        {
            ressource.gameObject.layer = LayerMask.NameToLayer("Grid");
            ressource.gameObject.isStatic = true;
            if (orientation == HexOrientation.FlatTop)
            {
                ressource.Rotate(new Vector3(0, 30, 0));
            }
            ressource.Rotate(new Vector3(0, randomRotation * 60, 0));
        }
    }

    public void SetNeighbours(List<HexCell> _neighbours)
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
}
