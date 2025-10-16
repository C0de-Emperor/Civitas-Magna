using System;
using UnityEngine;
using System.Collections.Generic;

public class SelectionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private HexGrid grid;
    [SerializeField]
    private GameObject selectionOutline;
    [SerializeField]
    private GameObject innerSelectionOutline;

    [Header("Data")]
    [HideInInspector]
    public HexCell outlinedCell = null;
    [HideInInspector, NonSerialized]
    public HexCell selectedCell = null;
    [HideInInspector, NonSerialized]
    public Transform selectedUnit = null;

    public static SelectionManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de SelectionManager dans la scene");
            return;
        }
        instance = this;

        selectionOutline.SetActive(false);
        selectionOutline.transform.localScale *= grid.hexSize;
        selectionOutline.transform.rotation = grid.orientation == HexOrientation.FlatTop
            ? Quaternion.Euler(-90f, 30f, 0f)
            : Quaternion.Euler(-90f, 0f, 0f);

        innerSelectionOutline.SetActive(false);
        innerSelectionOutline.transform.localScale *= grid.hexSize;
        innerSelectionOutline.transform.rotation = grid.orientation == HexOrientation.FlatTop
            ? Quaternion.Euler(-90f, 30f, 0f)
            : Quaternion.Euler(-90f, 0f, 0f);
    }

    private void Update()
    {
        if (selectionOutline == null || innerSelectionOutline == null || MapGenerator.instance.isMapReady == false)
            return;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Vector2 coord = HexMetrics.CoordinateToOffset(hit.point.x, hit.point.z, grid.hexSize, grid.orientation);
            HexCell currentCell = grid.GetTile(coord);
            if (currentCell != null && currentCell.tile != null)
            {
                if (currentCell != outlinedCell)
                {
                    outlinedCell = currentCell;

                    selectionOutline.SetActive(true);
                    selectionOutline.transform.position = new Vector3(
                        currentCell.tile.position.x,
                        (currentCell.isRevealed ? currentCell.terrainHigh : grid.undiscoveredTileHigh) + 0.001f,
                        currentCell.tile.position.z
                    );
                }

                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log(currentCell.offsetCoordinates);
                    if(currentCell.militaryUnit != null && (selectedUnit == null || currentCell.militaryUnit.gameObject != selectedUnit.gameObject))
                    {
                        selectedUnit = currentCell.militaryUnit;
                    }
                    else if(currentCell.supportUnit != null && (selectedUnit == null || currentCell.supportUnit.gameObject != selectedUnit.gameObject))
                    {
                        selectedUnit = currentCell.supportUnit;
                    }
                    // autre

                    grid.RevealTilesInRadius(coord, 2);

                    selectedCell = currentCell;
                    innerSelectionOutline.SetActive(true);
                    innerSelectionOutline.transform.position = new Vector3(
                        currentCell.tile.position.x,
                        (currentCell.isRevealed ? currentCell.terrainHigh : grid.undiscoveredTileHigh) + 0.001f,
                        currentCell.tile.position.z
                    );
                }

                
                //if (grid.GetTile(coord).prop != null)
                //    Destroy(grid.GetTile(coord).prop.gameObject);
                //Destroy(grid.GetTile(coord).terrain.gameObject);
            }
            else
            {
                outlinedCell = null;
                selectionOutline.SetActive(false);
            }
        }
        else
        {
            outlinedCell = null;
            selectionOutline.SetActive(false);
        }
       
        if (Input.GetKeyDown(KeyCode.Q))
        {
             selectedCell = null;
             innerSelectionOutline.SetActive(false);
        }
        if (Input.GetKeyUp(KeyCode.U) && selectedCell!=null)
        {
            grid.AddMilitaryUnit(selectedCell, UnitManager.instance.MilitaryUnits[0]);
        }
        if (Input.GetKeyUp(KeyCode.V))
        {
            List<Vector2> path = UnitManager.instance.GetShortestPath(grid, grid.GetTile(new Vector2(0, 0)), selectedCell, 1);
            foreach (var item in path)
            {
                Debug.Log(item.x+", "+item.y);
            }
        }
        
    }
}
