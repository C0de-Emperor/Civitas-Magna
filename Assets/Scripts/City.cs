using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CityBorders))]
public class City : MonoBehaviour
{
    public Dictionary<Vector2, HexCell> controlledTiles = new Dictionary<Vector2, HexCell>();

    public CityBorders borders;
}
