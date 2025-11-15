using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CivilianUnit", menuName = "Scriptable Objects/Unit/CivilianUnit")]
public class CivilianUnitType : UnitType
{
    public enum CivilianJob { Settler, Builder };

    [Header("Job specifications")]
    public CivilianJob job;
    public List<Building> buildableBuildings = new List<Building>();
    public int actionCharges;

    private void Awake()
    {
        unitCategory = UnitCategory.civilian;
    }
}
