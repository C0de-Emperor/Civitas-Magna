using UnityEngine;

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
}
