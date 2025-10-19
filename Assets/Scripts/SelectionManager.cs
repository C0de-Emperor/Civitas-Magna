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
    [HideInInspector, NonSerialized]
    public City selectedCity = null;

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

                    // --- Sélection d’une unité militaire ---
                    if (currentCell.militaryUnit != null)
                    {
                        if (selectedUnit == null || currentCell.militaryUnit.gameObject != selectedUnit.gameObject)
                        {
                            selectedUnit = currentCell.militaryUnit;
                        }
                        else if (currentCell.supportUnit != null)
                        {
                            // Sélectionne l’unité de support si elle est différente
                            if (selectedUnit.gameObject != currentCell.supportUnit.gameObject)
                                selectedUnit = currentCell.supportUnit;
                            else
                                selectedUnit = null;
                        }
                        else
                        {
                            selectedUnit = null;
                        }
                    }

                    // --- Sélection d’une unité de support uniquement ---
                    else if (currentCell.supportUnit != null)
                    {
                        if (selectedUnit == null || selectedUnit.gameObject != currentCell.supportUnit.gameObject)
                            selectedUnit = currentCell.supportUnit;
                        else
                            selectedUnit = null;
                    }

                    // --- Sélection d’une ville ---
                    else if (currentCell.isACity)
                    {
                        City city;

                        // Vérifie que la ville existe dans le dictionnaire
                        if (BuildingManager.instance.cities.TryGetValue(coord, out city))
                        {
                            if (selectedCity == null || selectedCity != city)
                            {
                                selectedCity = city;
                                Debug.Log("Ville sélectionnée : " + coord);
                            }
                            else
                            {
                                selectedCity = null;
                                Debug.Log("Ville désélectionnée");
                            }
                        }
                    }

                    // --- Rien à sélectionner ---
                    else
                    {
                        selectedUnit = null;
                        selectedCity = null;
                    }
                    grid.RevealTilesInRadius(coord, 2);

                   

                    selectedCell = currentCell;
                    innerSelectionOutline.SetActive(true);
                    innerSelectionOutline.transform.position = new Vector3(
                        currentCell.tile.position.x,
                        (currentCell.isRevealed ? currentCell.terrainHigh : grid.undiscoveredTileHigh) + 0.001f,
                        currentCell.tile.position.z
                    );
                }
                if (Input.GetMouseButtonDown(1))
                {
                    BuildingManager.instance.CreateCity(currentCell);
                }
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
