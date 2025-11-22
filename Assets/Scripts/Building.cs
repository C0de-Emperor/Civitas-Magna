using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/Building")]
public class Building : ScriptableObject
{
    public enum BuildingNames { None, City, Farm, Mine, Sawmill }

    [Header("Basic Properties")]
    public BuildingNames buildingName;
    public GameObject buildingPrefab;
    public Sprite buildingSprite;

    [Header("Building Factors")]
    public float foodFactor;
    public float productionFactor;

    [Header("Research")]
    public Research requiredResearch;

    public bool CanBeBuildOn(HexCell cell)
    {
        if (cell.building.buildingName != BuildingNames.None)
            return false;

        if (!cell.terrainType.build.Contains(buildingName))
            return false;

        if (requiredResearch != null &&
            !ResearchManager.instance.researched.Contains(requiredResearch))
            return false;

        return true;
    }
}
