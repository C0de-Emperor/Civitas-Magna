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

    [HideInInspector] public float militaryPower
    {
        get
        {
            float militaryPower = AttackPower * 1 + DefensePower * 1 + MaxHealth * 0.5f + AttackRange * 0.5f;
            if (IsABoat) { militaryPower += 3; }
            return militaryPower;
        }
        private set { }
    }

    private void Awake()
    {
        unitCategory = UnitCategory.military;
    }

}
