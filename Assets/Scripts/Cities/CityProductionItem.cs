using System.Collections.Generic;
using UnityEngine;

public abstract class CityProductionItem : ScriptableObject
{
    public string itemName;
    public float costInProduction;
    public Sprite icon;

    [Header("Requirements")]
    public Research requiredReserch;

    public abstract void OnProductionComplete(City city);
}