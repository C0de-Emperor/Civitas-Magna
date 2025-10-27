using UnityEngine;

[CreateAssetMenu(fileName = "CivilianUnit", menuName = "Scriptable Objects/CivilianUnit")]
public class CivilianUnitType : UnitType
{


    private void Awake()
    {
        unitCategory = UnitCategory.support;
    }
}
