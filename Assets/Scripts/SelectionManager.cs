using System;
using UnityEngine;
using System.Collections.Generic;

public class SelectionManager : MonoBehaviour
{
    public bool canInteract = true;


    [Header("References")]
    [SerializeField] private HexGrid grid;
    [SerializeField] private GameObject selectionOutline;
    [SerializeField] private GameObject innerSelectionOutline;


    [Header("Data")]
    [HideInInspector] public HexCell outlinedCell = null;
    [HideInInspector, NonSerialized] public HexCell selectedCell = null;
    [HideInInspector, NonSerialized] public Unit selectedUnit = null;


    private HexCell lastClickedCell;
    private int clickCycleIndex = 0;
    public bool showOverlay = false;

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
        // Unselect city
        if (Input.GetKeyDown(KeyCode.Escape) && CityManager.instance.openedCity != null)
        {
            CityManager.instance.CloseCity();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (showOverlay)
                grid.HideAllOverlay();
            else
                grid.ShowAllOverlay();

            showOverlay = !showOverlay;
        }

        if (selectionOutline == null || innerSelectionOutline == null || !MapGenerator.instance.isMapReady || !canInteract)
        {
            if(selectionOutline.gameObject.activeSelf)
                selectionOutline.gameObject.SetActive(false);

            return;
        }

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
                    HandleCellClick(currentCell, coord);

                    grid.RevealTilesInRadius(coord, 2, showOverlay);

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
                    if (selectedUnit != null)
                    {
                        UnitManager.instance.QueueUnitMovement(selectedCell, currentCell, selectedUnit.unitType.unitCategory);
                    }
                    else
                    {
                        CityManager.instance.CreateCity(currentCell);
                    }
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
       
        // DEBUG

        // Unselect
        if (Input.GetKeyDown(KeyCode.Q))
        {
             selectedCell = null;
             selectedUnit = null;
             innerSelectionOutline.SetActive(false);
        }



        // Debug Unit
        if (Input.GetKeyUp(KeyCode.U) && selectedCell!=null)
        {
            UnitManager.instance.AddUnit(UnitManager.instance.militaryUnits[0], selectedCell, new Player("player1", new Color[]{ new Color(255, 0, 0), new Color(0, 0, 0) }));
        }
        if (Input.GetKeyUp(KeyCode.I) && selectedCell != null)
        {
            UnitManager.instance.AddUnit(UnitManager.instance.militaryUnits[0], selectedCell, new Player("player2", new Color[] { new Color(0, 255, 0), new Color(255, 255, 255) }));
        }
        if (Input.GetKeyUp(KeyCode.O) && selectedCell != null)
        {
            UnitManager.instance.AddUnit(UnitManager.instance.militaryUnits[1], selectedCell, new Player("player3", new Color[] { new Color(0, 0, 0), new Color(255, 0, 255) }));
        }
        if (Input.GetKeyUp(KeyCode.P))
        {
            TurnManager.instance.ChangeTurn();
        }
    }

    void HandleCellClick(HexCell currentCell, Vector2 coord)
    {
        if (currentCell == null)
            return;

        // Si on clique sur une nouvelle cellule - reset le cycle
        if (lastClickedCell != currentCell)
        {
            lastClickedCell = currentCell;
            clickCycleIndex = 0;
        }

        // Crée une liste dynamique des éléments disponibles sur la cellule
        List<System.Action> actions = new List<System.Action>();

        // Ajoute une action pour l’unité militaire
        if (currentCell.militaryUnit != null)
        {
            actions.Add(() =>
            {
                selectedUnit = currentCell.militaryUnit;
            });
        }

        // Ajoute une action pour l’unité de support
        if (currentCell.civilianUnit != null)
        {
            actions.Add(() =>
            {
                selectedUnit = currentCell.civilianUnit;
            });
        }

        // Ajoute une action pour la ville
        if (currentCell.isACity)
        {
            actions.Add(() =>
            {
                if (CityManager.instance.cities.TryGetValue(coord, out City city))
                {
                    CityManager.instance.OpenCity(city);
                }
            });
        }

        // Si aucun élément sur la cellule
        if (actions.Count == 0)
        {
            selectedUnit = null;
            lastClickedCell = null;
            clickCycleIndex = 0;
            return;
        }

        // Exécute l’action correspondante
        actions[clickCycleIndex].Invoke();

        // Passe à l’élément suivant, en bouclant
        clickCycleIndex = (clickCycleIndex + 1) % actions.Count;
    }
}
