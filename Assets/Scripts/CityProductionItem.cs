using UnityEngine;

public abstract class CityProductionItem : ScriptableObject
{
    public string itemName;
    public float costInProduction;
    public Sprite icon;

    public abstract void OnProductionComplete(City city);
}