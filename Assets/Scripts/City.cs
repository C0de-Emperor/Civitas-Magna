using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CityBorders))]
public class City : MonoBehaviour
{
    public string cityName = "Default";

    [Header("Production")]
    public float food = 0.0f;
    public float production = 0.0f;
    public float gold = 0.0f;
    public float science = 0.0f;


    [HideInInspector] public Dictionary<Vector2, HexCell> controlledTiles = new Dictionary<Vector2, HexCell>();
    [HideInInspector] public CityBorders borders;
}
