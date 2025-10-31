using UnityEngine;

public abstract class CityProductionItem : ScriptableObject
{
    public string itemName;
    public float cost;
    public Sprite icon;

    public abstract void OnProductionComplete(City city);
}