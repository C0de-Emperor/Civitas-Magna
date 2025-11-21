using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuildActionButton : MonoBehaviour
{
    [HideInInspector] public Building building;

    public Button button;
    public Image icon;

    internal void UpdateButton(HexCell cell)
    {
        if (cell.building.buildingName == building.buildingName || !cell.terrainType.build.Contains(building.buildingName))
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
        }
    }
}
