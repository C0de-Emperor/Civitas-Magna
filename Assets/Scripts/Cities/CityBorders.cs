using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(City))]
public class CityBorders : MonoBehaviour
{
    public City city;
    public Transform edgesParent;
    public float lineHeight = 0.05f;
    public float lineWidth = 0.05f;

    private Material borderMaterial;

    private void SetMaterial()
    {
        borderMaterial = new Material(Shader.Find("Unlit/Color"));
        borderMaterial.color = city.master.livery.spriteColor;
    }

    public void UpdateBorders()
    {
        if (city == null || city.controlledTiles.Count == 0)
            return;

        if (borderMaterial == null)
            SetMaterial();

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
                if (cell.neighbours != null && i < 6)
                    neighbor = cell.neighbours[i];

                Vector2Int neighborCoord = neighbor != null ? neighbor.offsetCoordinates : new Vector2Int(-1, -1);


                bool isOutsideCity = neighbor == null || !city.controlledTiles.ContainsKey(neighborCoord);

                bool isNotSamePlayer = !CityManager.instance.IsToAPlayer(neighbor, city.master);

                bool isRevealed = cell.isRevealed;

                if (isOutsideCity && isNotSamePlayer && isRevealed)
                {
                    float neighborHeight = neighbor != null ? neighbor.terrainHigh : 0f;
                    float maxHeight = Mathf.Max(cell.terrainHigh, neighborHeight) + 0.05f;

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

                    //lr.SetPosition(0, corners[a] + Vector3.up * maxHeight);
                    //lr.SetPosition(1, corners[b] + Vector3.up * maxHeight);

                    float inwardOffset = 0.05f; // à ajuster selon la taille de ton hex

                    Vector3 center = cell.tile.transform.position;

                    // Direction vers l’intérieur
                    Vector3 inwardDirA = (center - corners[a]).normalized;
                    Vector3 inwardDirB = (center - corners[b]).normalized;

                    // Nouvelle position légèrement vers l’intérieur
                    Vector3 posA = corners[a] + inwardDirA * inwardOffset + Vector3.up * maxHeight;
                    Vector3 posB = corners[b] + inwardDirB * inwardOffset + Vector3.up * maxHeight;

                    lr.SetPosition(0, posA);
                    lr.SetPosition(1, posB);
                }
            }
        }
    }

    public void ExpandCity()
    {
        int tileAmount = GetWeightedRandom();

        for (int i = 0; i < tileAmount; i++)
        {
            HexCell newTile = GetRandomBorderTile(city);
            if (newTile == null)
            {
                Debug.LogWarning($"{city.cityName} n’a plus de tuiles disponibles pour s’étendre.");
                break;
            }

            // Ajoute la tuile à la ville
            if(city.master == PlayerManager.instance.player)
                newTile.grid.RevealTilesInRadius(newTile.offsetCoordinates, 2, SelectionManager.instance.showOverlay, true);

            city.controlledTiles.Add(newTile.offsetCoordinates, newTile);
            CityManager.instance.tileToCity.Add(newTile.offsetCoordinates, city);
        }
        // UpdateBorders();
    }

    private int GetWeightedRandom()
    {
        float r = Random.value;

        if (r < 0.8f) return 1;      // 80%
        else if (r < 0.95f) return 2; // 15%
        else return 3;               // 5%
    }

    private static HexCell GetRandomBorderTile(City city)
    {
        if (city == null || city.controlledTiles == null || city.controlledTiles.Count == 0)
            return null;

        HashSet<HexCell> borderTiles = new HashSet<HexCell>();

        foreach (HexCell tile in city.controlledTiles.Values)
        {
            foreach (HexCell neighbor in tile.neighbours)
            {
                if (neighbor == null)
                    continue;

                // déjà contrôlées
                if (city.controlledTiles.ContainsKey(neighbor.offsetCoordinates))
                    continue;

                // appartient déjà à une autre ville
                if (CityManager.instance.IsToACity(neighbor))
                    continue;

                float distance = UnitManager.instance.GetDistance(city.occupiedCell, neighbor); 
                //HexMetrics.GetDistance(city.occupiedCell.offsetCoordinates, neighbor.offsetCoordinates);
                if (distance > CityManager.instance.maxCityRadius)
                    continue;

                borderTiles.Add(neighbor);
            }
        }

        if (borderTiles.Count == 0)
            return null;

        int index = Random.Range(0, borderTiles.Count);
        int i = 0;
        foreach (var tile in borderTiles)
        {
            if (i == index)
                return tile;
            i++;
        }

        return null; // si ça retourne null c'est pas censé
    }
}
