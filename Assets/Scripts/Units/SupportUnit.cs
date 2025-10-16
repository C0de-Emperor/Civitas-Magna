using UnityEngine;

[CreateAssetMenu(fileName = "SupportUnit", menuName = "Scriptable Objects/SupportUnit")]
public class SupportUnit : ScriptableObject
{
    public enum UnitType { }

    [Header("Basic Properties")]
    [SerializeField] private int ID;
    [SerializeField] public string Name;
    [SerializeField] public Transform Prefab;
    [SerializeField] public int MoveReach;
    [SerializeField] public bool IsABoat;

}
