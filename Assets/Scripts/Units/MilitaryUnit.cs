using UnityEngine;

[CreateAssetMenu(fileName = "MilitaryUnit", menuName = "Scriptable Objects/MilitaryUnit")]
public class MilitaryUnit : ScriptableObject
{
    public enum UnitType { }

    [Header("Basic Properties")]
    [SerializeField] private int ID;
    [SerializeField] public string Name;
    [SerializeField] public Transform Prefab;
    [SerializeField] public int MoveReach;
    [SerializeField] public bool IsABoat;

    [Header("Combat properties")]
    [SerializeField] public float AttackPower;
    [SerializeField] public float DefensePower;
    [SerializeField] public float MaxHealth;
    [SerializeField] public int AttackRange;

}
