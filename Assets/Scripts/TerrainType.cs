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
    [field: SerializeField] public Sprite icon { get; private set; }
}
