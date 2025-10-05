using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public List<TerrainType> terrainTypes = new List<TerrainType>();

    public static ResourceManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de ResourceManager dans la scène");
            return;
        }
        instance = this;
    }
}