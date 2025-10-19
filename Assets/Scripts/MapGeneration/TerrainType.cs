using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TerrainType", menuName = "TBS/TerrainType")]
public class TerrainType : ScriptableObject
{
    [Header("Basic Properties")]
    [SerializeField] private int ID;
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public string description { get; private set; }
    [field: SerializeField] public Color color { get; private set; }
    [field: SerializeField] public Transform prefab { get; private set; }
    [field: SerializeField] public Transform prop { get; private set; }
    [field: SerializeField] public float terrainCost { get; private set; }
    [field: SerializeField] public bool traversable { get; private set; }

    [SerializeField] public List<Build> build = new List<Build> ();

    public enum Build { City }
}
