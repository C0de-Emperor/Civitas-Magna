using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TerrainType", menuName = "Scriptable Objects/TerrainType")]
public class TerrainType : ScriptableObject
{
    [Header("Basic Properties")]
    [SerializeField] public int ID;
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public Transform prefab { get; private set; }
    [field: SerializeField] public Transform prop { get; private set; }
    [field: SerializeField] public float terrainCost { get; private set; } 
    [field: SerializeField] public bool isWater { get; private set; }
    [field: SerializeField] public List<Building.BuildingNames> build { get; private set; } = new List<Building.BuildingNames>();

    [Header("Base Ressources")]
    [field: SerializeField] public int food;
    [field: SerializeField] public int production;
}
