using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "MilitaryUnit", menuName = "Scriptable Objects/MilitaryUnit")]
public class MilitaryUnitType : UnitType
{
    public enum UnitType { }

    [SerializeField] public UnitType Type;

    [Header("Combat properties")]
    [SerializeField] public float AttackPower;
    [SerializeField] public float DefensePower;
    [SerializeField] public float MaxHealth;
    [SerializeField] public int AttackRange;

    private void Awake()
    {
        unitCategory = UnitCategory.military;
    }

}
