using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private GameObject cityPrefab;

    public static BuildingManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de BuildingManager dans la scène");
            return;
        }
        instance = this;
    }

    public void CreateCity(HexCell cell, HexGrid grid)
    {

    }
}
