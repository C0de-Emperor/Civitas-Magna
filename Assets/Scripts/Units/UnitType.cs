using UnityEngine;

public class UnitType : ScriptableObject
{
    public enum UnitCategory { military, civilian };

    [Header("Basic Properties")]
    public GameObject unitPrefab;
    public Sprite unitSprite;
    public Sprite unitIcon;
    public int MoveReach;
    public int tileRevealRadius;
    public bool IsABoat;

    public int baseProductionCost;

    [HideInInspector] public UnitCategory unitCategory;
}
