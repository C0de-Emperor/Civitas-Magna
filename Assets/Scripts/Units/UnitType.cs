using UnityEngine;

public class UnitType : ScriptableObject
{
    public enum UnitCategory { military, support };

    [Header("Basic Properties")]
    [SerializeField] public string Name;
    [SerializeField] public Transform Prefab;
    [SerializeField] public int MoveReach;
    [SerializeField] public bool IsABoat;
    [HideInInspector] public UnitCategory unitCategory;
}
