using System.Collections.Generic;
using UnityEngine;

public class UnitType : ScriptableObject
{
    public enum UnitCategory { military, civilian };

    [Header("Basic Properties")]
    public GameObject unitPrefab;
    public Sprite unitSprite;
    public Sprite unitIcon;
    public int MoveReach;
    public int sightRadius;
    public bool IsABoat;

    public int baseProductionCost;
    public int baseGoldCost;

    public List<TerrainType> speciallyAccessibleTerrains = new List<TerrainType>();

    [HideInInspector] public UnitCategory unitCategory;
}
