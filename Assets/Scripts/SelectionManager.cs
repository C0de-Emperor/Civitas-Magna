using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour
{
    public bool canInteract = true;


    [Header("References")]
    public HexGrid grid;
    [SerializeField] private GameObject selectionOutline;
    [SerializeField] private GameObject innerSelectionOutline;
    [SerializeField] private GameObject pathPreviewLine;
    [SerializeField] private GameObject queuedPathPreviewLine;


    [Header("Data")]
    [HideInInspector] public HexCell outlinedCell = null;
    [HideInInspector, NonSerialized] public HexCell selectedCell = null;
    [HideInInspector, NonSerialized] public Unit selectedUnit = null;


    [SerializeField] public Transform pathPreviewContainer = null;
    [SerializeField] public Transform queuedPathPreviewContainer = null;
    [HideInInspector] public List<Vector3> pathPreviewCoordinates = new List<Vector3>();
    [HideInInspector] public List<Vector3> queuedPathPreviewCoordinates = new List<Vector3>();

    private HexCell lastClickedCell;
    private int clickCycleIndex = 0;
    public bool showOverlay = false;
    public float pathLineOffset = 0.1f;

    public static SelectionManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de SelectionManager dans la scene");
            return;
        }
        instance = this;

        pathPreviewCoordinates.Clear();

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
            selectedCell = null;
        }

        // Close Science Menu
        if (Input.GetKeyDown(KeyCode.Escape) && ResearchManager.instance.isMenuOpen)
        {
            ResearchManager.instance.CloseMenu();
            if (outlinedCell != null || selectedCell != null || selectedUnit != null)
            {
                selectionOutline.gameObject.SetActive(false);
                innerSelectionOutline.gameObject.SetActive(false);
                selectedCell = null;
                outlinedCell = null;
                selectedUnit = null;
            }
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
            if (outlinedCell != null || selectedCell != null || selectedUnit != null)
            {
                selectionOutline.gameObject.SetActive(false);
                innerSelectionOutline.gameObject.SetActive(false);
                selectedCell = null;
                outlinedCell = null;
                selectedUnit = null;
            }
            
            return;
        }

        pathPreviewCoordinates = new List<Vector3>();
        queuedPathPreviewCoordinates = new List<Vector3>();

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

                if (selectedUnit != null)
                {
                    pathPreviewCoordinates = GetPathCoordinates(UnitManager.instance.GetShortestPath(selectedCell, currentCell, selectedUnit.unitType));
                }

                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    HandleCellClick(currentCell, coord);

                    grid.RevealTilesInRadius(coord, 10, showOverlay, false); // DEVELOPPEMENT, A ENLEVER

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
                        if(UnitManager.instance.QueueUnitMovement(selectedCell, currentCell, selectedUnit.unitType.unitCategory))
                        {
                            if (selectedUnit.unitType.unitCategory == UnitType.UnitCategory.civilian)
                            {
                                selectedUnit.unitCanvaTransform.gameObject.SetActive(false);
                            }

                            selectedCell = null;
                            selectedUnit = null;
                            pathPreviewCoordinates = new List<Vector3>();
                        }
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

        if (selectedUnit != null && UnitManager.instance.queuedUnitMovements.ContainsKey(selectedUnit.id))
        {
            queuedPathPreviewCoordinates = GetPathCoordinates(UnitManager.instance.queuedUnitMovements[selectedUnit.id].path);
        }
        DrawPathPreview(pathPreviewCoordinates, pathPreviewContainer, pathPreviewLine);
        DrawPathPreview(queuedPathPreviewCoordinates, queuedPathPreviewContainer, queuedPathPreviewLine);

        // Debug Unit, à dégager
        if (Input.GetKeyUp(KeyCode.U) && selectedCell!=null)
        {
            UnitManager.instance.AddUnit(UnitManager.instance.militaryUnits[0], selectedCell, PlayerManager.instance.player);
        }
        if (Input.GetKeyUp(KeyCode.I) && selectedCell != null)
        {
            UnitManager.instance.AddUnit(UnitManager.instance.militaryUnits[0], selectedCell, new Player("player2", new Color[] { new Color(0, 255, 0), new Color(255, 255, 255) }));
        }
        if (Input.GetKeyUp(KeyCode.O) && selectedCell != null)
        {
            UnitManager.instance.AddUnit(UnitManager.instance.militaryUnits[1], selectedCell, new Player("player3", new Color[] { new Color(0, 0, 0), new Color(255, 0, 255) }));
        }
		if (Input.GetKeyUp(KeyCode.J) && selectedCell != null)
		{
			UnitManager.instance.AddUnit(UnitManager.instance.militaryUnits[2], selectedCell, new Player("player3", new Color[] { new Color(0, 0, 0), new Color(255, 0, 255) }));
		}
		if (Input.GetKeyUp(KeyCode.K) && selectedCell != null)
		{
			UnitManager.instance.AddUnit(UnitManager.instance.militaryUnits[2], selectedCell, new Player("player3", new Color[] { new Color(0, 123, 67), new Color(4, 89, 176) }));
		}
		if (Input.GetKeyUp(KeyCode.X) && selectedCell != null)
		{
			UnitManager.instance.AddUnit(UnitManager.instance.civilianUnits[0], selectedCell, PlayerManager.instance.player);
		}
        if (Input.GetKeyUp(KeyCode.C) && selectedCell != null)
        {
            UnitManager.instance.AddUnit(UnitManager.instance.civilianUnits[1], selectedCell, PlayerManager.instance.player);
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
        if (selectedUnit != null && selectedUnit.unitType.unitCategory == UnitType.UnitCategory.civilian)
        {
			selectedUnit.unitCanvaTransform.gameObject.SetActive(false);
		}


        // Si on clique sur une nouvelle cellule - reset le cycle
        if (lastClickedCell != currentCell)
        {
            lastClickedCell = currentCell;
            clickCycleIndex = 0;
        }

        // Crée une liste dynamique des éléments disponibles sur la cellule
        List<Action> actions = new List<Action>();

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
				selectedUnit.unitCanvaTransform.gameObject.SetActive(true);
            });
        }

        // Ajoute une action pour la ville
        if (currentCell.building.buildingName == Building.BuildingNames.City)
        {
            selectedUnit = null;
            actions.Add(() =>
            {
                if (CityManager.instance.cities.TryGetValue(coord, out City city))
                {
                    Debug.Log("e");
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

        if (clickCycleIndex >= actions.Count)
            clickCycleIndex = 0;

        // Exécute l’action correspondante
        actions[clickCycleIndex].Invoke();

        // Passe à l’élément suivant, en bouclant
        clickCycleIndex = (clickCycleIndex + 1) % actions.Count;
    }

    public List<Vector3> GetPathCoordinates(List<HexCell> path)
    {
        List<Vector3> pathCoordinates = new List<Vector3>();

        if (path != null)
        {
            foreach (HexCell cell in path)
            {
                pathCoordinates.Add(new Vector3(cell.tile.position.x, cell.terrainHigh + pathLineOffset, cell.tile.position.z));
            }
        }

        return pathCoordinates;
    }

    public void DrawPathPreview(List<Vector3> pathCoordinates, Transform pathContainer, GameObject linePrefab)
    {
        Transform currentLine = null;
        for(int i=0;  i<pathCoordinates.Count-1; i++)
        {
            if (i < pathContainer.childCount)
            {
                currentLine = pathContainer.GetChild(i);
            }
            else
            {
                currentLine = Instantiate(linePrefab, pathContainer).transform;
                currentLine.localScale = new Vector3(1, 1, grid.hexSize*1.75f);
            }

            currentLine.position = pathCoordinates[i];
            currentLine.LookAt(pathCoordinates[i+1]);
        }

        if (pathCoordinates.Count > 0 && pathCoordinates.Count-1< pathContainer.childCount)
        {
            Destroy(pathContainer.GetChild(pathCoordinates.Count - 1).gameObject);
        }
        for (int i = pathCoordinates.Count; i < pathContainer.childCount; i++)
        {
            Destroy(pathContainer.GetChild(i).gameObject);
        }
    }
}