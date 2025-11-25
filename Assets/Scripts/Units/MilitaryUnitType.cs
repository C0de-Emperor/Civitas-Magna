using UnityEngine;

[CreateAssetMenu(fileName = "MilitaryUnit", menuName = "Scriptable Objects/Unit/MilitaryUnit")]
public class MilitaryUnitType : UnitType
{
    [Header("Combat properties")]
    public float AttackPower;
    public float DefensePower;
    public float MaxHealth;
    public int AttackRange;
    public float HealthRegeneration;

    [Header("IA properties")]
    public float militaryPower;

    private void Awake()
    {
        unitCategory = UnitCategory.military;
    }

}
