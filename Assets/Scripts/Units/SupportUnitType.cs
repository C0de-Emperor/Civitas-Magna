using UnityEngine;

[CreateAssetMenu(fileName = "SupportUnit", menuName = "Scriptable Objects/SupportUnit")]
public class SupportUnitType : UnitType
{


    private void Awake()
    {
        unitCategory = UnitCategory.support;
    }
}
