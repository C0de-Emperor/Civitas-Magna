using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(HexGrid))]
public class MapGenerator : MonoBehaviour
{
    public HexGrid hexGrid;

    [Header("Noise Settings")]
    public float noiseScale = 0.5f;
    public int octaves = 6;
    [Range(0f, 1f)] public float persistance = 0.5f;
    public float lacunarity = 2f;
    public int seed = 0;
    public Vector2 offset = Vector2.zero;
    [Header("Water Level (0-1)")]
    [Range(0f, 1f)] public float seaLevel = 0.45f;

    [Header("Biomes")]
    public List<TerrainHeight> biomes = new List<TerrainHeight>();

    public bool useThreadedGeneration = true;

    public static MapGenerator instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de MapGenerator dans la scène");
            return;
        }
        instance = this;

        seed = UnityEngine.Random.Range(-10000, 10000);

        hexGrid = GetComponent<HexGrid>();

        // Tri des biomes par hauteur
        biomes.Sort((a, b) => a.height.CompareTo(b.height));
    }

    private void Start() => GenerateMap();

    public void GenerateMap()
    {
        int width = hexGrid.width;
        int height = hexGrid.height;

        Action generateAction = () =>
        {
            // --- 1 Grande échelle : les continents ---
            float[,] continentNoise = Noise.GenerateNoiseMap(
                width, height,
                noiseScale * 7f,  // très grand = continents larges
                seed - 200,
                2,                   // peu d’octaves = formes douces
                persistance,
                2f,
                offset
            );

            // --- 2 Petite échelle : les détails de terrain ---
            float[,] detailNoise = Noise.GenerateNoiseMap(
                width, height,
                noiseScale * 0.4f,    // petits motifs = relief local
                seed + 100,
                octaves,
                persistance * 2f,
                lacunarity,
                offset
            );

            TerrainType[,] terrainMap = new TerrainType[width, height];

            // --- 3 Combine les deux bruits intelligemment ---
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float c = continentNoise[x, y];
                    float d = detailNoise[x, y];

                    TerrainType chosen;

                    // === 1 Océan profond ===
                    if (c < seaLevel - 0.05f)
                    {
                        chosen = biomes[0].terrainType; // Ocean
                    }
                    // === 2 Zone côtière (sable, plage, marais, etc.) ===
                    else if (c < seaLevel)
                    {
                        // on est dans la bande de transition (10% autour du niveau de la mer)
                        chosen = biomes[1].terrainType;
                    }
                    // === 3 Terre ferme ===
                    else
                    {
                        // On calcule un bruit local basé sur le détail
                        float local = Mathf.Clamp01(d);

                        // On cherche le biome terrestre correspondant (sans toucher à l’eau/sable)
                        chosen = biomes[biomes.Count - 1].terrainType;
                        foreach (var biome in biomes)
                        {
                            // on ignore les biomes d'eau et de sable ici
                            if (biome.height <= 0.45f) continue; // par exemple: eau/sable

                            if (local <= biome.height)
                            {
                                chosen = biome.terrainType;
                                break;
                            }
                        }
                    }

                    terrainMap[x, y] = chosen;
                    //colorMap[y * width + x] = chosen.color;
                }
            }

            // Retour sur le main thread
            MainThreadDispatcher.instance.Enqueue(() =>
            {
                List<HexCell> cells = new List<HexCell>();
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        HexCell cell = new HexCell();
                        cell.SetCoordinates(new Vector2(x, y), hexGrid.orientation);
                        cell.terrainhight = Mathf.Clamp01(
                            continentNoise[x, y] * 0.5f + detailNoise[x, y] * 0.5f
                        );
                        cell.hexSize = hexGrid.hexSize;
                        cell.SetTerrainType(terrainMap[x, y]);
                        cells.Add(cell);
                    }
                }
                hexGrid.SetHexCells(cells);
            });
        };

        if (useThreadedGeneration)
            Task.Run(generateAction);
        else
            generateAction.Invoke();
    }
}

[System.Serializable]
public struct TerrainHeight
{
    public float height; // 0-1
    public TerrainType terrainType;
}
