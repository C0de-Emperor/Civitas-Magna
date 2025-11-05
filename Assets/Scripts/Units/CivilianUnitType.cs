using UnityEngine;

[CreateAssetMenu(fileName = "CivilianUnit", menuName = "Scriptable Objects/Unit/CivilianUnit")]
public class CivilianUnitType : UnitType
{


    private void Awake()
    {
        unitCategory = UnitCategory.civilian;
    }
}
