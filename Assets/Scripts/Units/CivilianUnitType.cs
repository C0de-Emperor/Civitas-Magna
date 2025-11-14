using UnityEngine;

[CreateAssetMenu(fileName = "CivilianUnit", menuName = "Scriptable Objects/Unit/CivilianUnit")]
public class CivilianUnitType : UnitType
{
    public enum CivilianJob { Settler, Builder };

    [Header("Job specifications")]
    public CivilianJob job;
    public int actionCharges;

    private void Awake()
    {
        unitCategory = UnitCategory.civilian;
    }
}
