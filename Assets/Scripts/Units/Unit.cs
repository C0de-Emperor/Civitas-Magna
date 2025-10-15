using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "Scriptable Objects/Unit")]
public class Unit : ScriptableObject
{
    [Header("Basic Properties")]
    [SerializeField] private int ID;
    [SerializeField] public string Name;
    [SerializeField] public Transform prefab;
    [SerializeField] public UnitType Type;
    public enum UnitType { military, support };
    [SerializeField] public UnitSubType SubType;
    public enum UnitSubType { Warior, Builder };
    [SerializeField] public int MoveReach;
    [SerializeField] public float AttackPower;
    [SerializeField] public float DefensePower;
    [SerializeField] public float MaxHealth;
    [SerializeField] public int AttackRange;

}
