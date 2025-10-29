using UnityEngine;

[CreateAssetMenu(fileName = "CivilianUnit", menuName = "Scriptable Objects/CivilianUnit")]
public class CivilianUnitType : UnitType
{


    private void Awake()
    {
        unitCategory = UnitCategory.support;
        unitName = names[Random.Range(0, names.Length - 1)];
    }
}
