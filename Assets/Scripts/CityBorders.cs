using System.Collections.Generic;
using UnityEngine;

public class CityBorders : MonoBehaviour
{
    public City city;
    public Transform edgesParent;
    public float lineHeight = 0.05f;
    public float lineWidth = 0.05f;
    public Color borderColor = Color.yellow;

    private Material borderMaterial;

    private void Awake()
    {
        borderMaterial = new Material(Shader.Find("Unlit/Color"));
        borderMaterial.color = borderColor;
    }

    public void UpdateBorders()
    {
        if (city == null || city.controlledTiles.Count == 0)
            return;

        // Supprimer les anciens segments
        foreach (Transform child in edgesParent)
            Destroy(child.gameObject);

        foreach (HexCell cell in city.controlledTiles.Values)
        {
            Vector3[] corners = HexMetrics.Corners(cell.hexSize, cell.orientation);
            for (int i = 0; i < 6; i++)
                corners[i] += cell.tile.transform.position;

            for (int i = 0; i < 6; i++) // on parcours les corneril y en a toujours 6
            {
                HexCell neighbor = null;
                if (cell.neighbours != null && i < cell.neighbours.Count)
                    neighbor = cell.neighbours[i];

                Vector2 neighborCoord = neighbor != null ? neighbor.offsetCoordinates : new Vector2(-1, -1);

                if ( (neighbor == null || !city.controlledTiles.ContainsKey(neighborCoord)) && !CityManager.instance.IsToACity(neighbor))
                {
                    float neighborHeight = neighbor != null ? neighbor.terrainHigh : 0f;
                    float maxHeight = Mathf.Max(cell.terrainHigh, neighborHeight);

                    int a = (i + 5) % 6; // i - 1
                    int b = i; // i

                    GameObject edge = new GameObject("Edge");
                    edge.transform.parent = edgesParent;
                    edge.isStatic = true;

                    LineRenderer lr = edge.AddComponent<LineRenderer>();
                    lr.material = borderMaterial;
                    lr.widthMultiplier = lineWidth;
                    lr.positionCount = 2;
                    lr.numCapVertices = 2;
                    lr.useWorldSpace = true;

                    lr.SetPosition(0, corners[a] + Vector3.up * maxHeight);
                    lr.SetPosition(1, corners[b] + Vector3.up * maxHeight);

                    
                }
            }
        }
    }
}
