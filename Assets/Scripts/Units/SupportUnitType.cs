using UnityEngine;

public class SupportUnitType : UnitType
{
    public enum UnitType { }

    [SerializeField] public UnitType Type;


    private void Awake()
    {
        unitCategory = UnitCategory.support;
    }
}
