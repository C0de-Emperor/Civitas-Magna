using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HexCell
{
    [Header("Cell Properties")]
    [SerializeField] private HexOrientation orientation;
    [field:SerializeField] public HexGrid grid {  get; set; }
    [field:SerializeField] public float hexSize {  get; set; }
    [field:SerializeField] public TerrainType terrainType {  get; private set; }
    [field:SerializeField] public float terrainhight {  get; set; }
    [field:SerializeField] public Vector2 offsetCoordinates {  get; set; }
    [field:SerializeField] public Vector3 cubeCoordinates {  get; private set; }
    [field:SerializeField] public Vector2 axialCoordinates {  get; private set; }
    [field: NonSerialized] public List<HexCell> neighbours { get; private set; }

    [field:SerializeField] public Transform terrain {  get; private set; }
    [field: SerializeField] public Transform prop { get; private set; }

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

        terrain = UnityEngine.Object.Instantiate(
            terrainType.prefab,
            centrePosition,
            Quaternion.identity,
            grid.transform
        );

        if(terrainType.prop != null)
        {
            prop = UnityEngine.Object.Instantiate(
                terrainType.prop,
                centrePosition + new Vector3(0, terrainhight, 0),
                Quaternion.identity,
                grid.transform
            );
        }


        Renderer rend = terrain.GetComponentInChildren<Renderer>();
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

            Vector3 newScale = terrain.transform.localScale;
            newScale.x = targetX / prefabSize.x * newScale.x;
            newScale.y = terrainhight;
            newScale.z = targetZ / prefabSize.z * newScale.z;
            terrain.transform.localScale = newScale;
            if (prop != null)
            {
                prop.transform.localScale = newScale;
            }
        }

        terrain.gameObject.layer = LayerMask.NameToLayer("Grid");
        terrain.gameObject.isStatic = true;
        
        if (orientation == HexOrientation.FlatTop)
        {
            terrain.Rotate(new Vector3(0, 30, 0));
        }

        int randomRotation = UnityEngine.Random.Range(0, 6);
        terrain.Rotate(new Vector3(0, randomRotation * 60, 0));
        
        if (prop != null)
        {
            prop.gameObject.layer = LayerMask.NameToLayer("Grid");
            prop.gameObject.isStatic = true;
            if (orientation == HexOrientation.FlatTop)
            {
                prop.Rotate(new Vector3(0, 30, 0));
            }
            prop.Rotate(new Vector3(0, randomRotation * 60, 0));
        }
    }

    public void SetNeighbours(List<HexCell> _neighbours)
    {
        neighbours = _neighbours;
    }

    public void ClearTerrain()
    {
        if (terrain != null)
        {
            UnityEngine.Object.Destroy(terrain.gameObject);
        }
    }
}
