using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Unity.Cinemachine;


public class CameraController : MonoBehaviour
{
    [SerializeField]
    private const int DEFAULT_PRIORITY = 10;

    [Header("References")]
    [SerializeField]
    private HexGrid grid;
    [SerializeField]
    private GameObject cameraTarget;
    [SerializeField]
    private CinemachineCamera topDownCamera;
    [SerializeField]
    private CinemachineCamera focusCamera;
    [SerializeField]
    private CameraMode defaultMode = CameraMode.TopDown;
    [SerializeField]
    private CameraMode currentMode;
    [SerializeField]
    private GameObject selectionOutline;
    [HideInInspector]
    public HexCell outlinedCell = null;

    [Header("Parameters")]
    [SerializeField] private float cameraSpeed = 10f;
    [SerializeField] private float cameraZoomSpeed = 1f;
    [SerializeField] private float cameraZoomMin = 15f;
    [SerializeField] private float cameraZoomMax = 100f;
    [SerializeField] private float cameraZoomDefault = 50f;


    private Coroutine panCoroutine;
    private Coroutine zoomCoroutine;

    public event Action<CinemachineCamera> onCameraChanged;

    public static CameraController instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de CameraController dans la scène");
            return;
        }
        instance = this;
    }

    void Start()
    {
        selectionOutline.SetActive(false);
        topDownCamera.Lens.FieldOfView = cameraZoomDefault;
        ChangeCamera(defaultMode);
    }

    public void ChangeCamera(CameraMode mode)
    {
        currentMode = mode;
        CinemachineCamera camera = GetCamera(mode);
        onCameraChanged?.Invoke(camera);
        camera.Priority = DEFAULT_PRIORITY;
    }

    private CinemachineCamera GetCamera(CameraMode mode)
    {
        switch (mode)
        {
            case CameraMode.TopDown:
                return topDownCamera;
            case CameraMode.Focus:
                return focusCamera;
            default:
                return null;
        }
    }


    public void OnPanChange(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (panCoroutine != null)
            {
                StopCoroutine(panCoroutine);
            }
            panCoroutine = StartCoroutine(ProcessPan(context));
        }
        else if (context.canceled)
        {
            if (panCoroutine != null)
            {
                StopCoroutine(panCoroutine);
            }
        }
        ChangeCamera(CameraMode.TopDown);
    }

    public void OnZoomChanged(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            //Debug.Log("Pressed Zoom key");
        }
        if (context.performed)
        {
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }
            zoomCoroutine = StartCoroutine(ProcessZoom(context));
        }
        else if (context.canceled)
        {
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }
        }
    }

    public void OnFocusChange(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            //Debug.Log("Focus button pressed... What's it gonna be?");
        }
        else if (context.performed)
        {
            //Debug.Log("Double tapped - Focus");
            ChangeCamera(CameraMode.Focus);
        }
        else if (context.canceled)
        {
            //Debug.Log("Single tap - Select");
        }
    }



    public IEnumerator ProcessPan(InputAction.CallbackContext context)
    {
        while (true)
        {
            //Move the camera target in the direction of the input (2D Vector)
            Vector2 inputVector = context.ReadValue<Vector2>();
            //Debug.Log("Moving: " + inputVector);

            //Move the camera target in the direction of the input (2D Vector)
            Vector3 moveVector = new Vector3(inputVector.x, 0, inputVector.y);
            cameraTarget.transform.position += moveVector * cameraSpeed * Time.deltaTime;

            yield return null;
        }
    }

    public IEnumerator ProcessZoom(InputAction.CallbackContext context)
    {
        //Change the FOV of the camera based on the input. If not keyboard, then adjust the value based on the scrollWheelZoomSpeed
        float zoomInput = context.ReadValue<float>();

        //Debug.Log("Zooming: " + zoomInput);
        while (true)
        {
            //Change the FOV of the camera based on the input. If not keyboard, then adjust the value based on the scrollWheelZoomSpeed
            float zoomAmount = topDownCamera.Lens.FieldOfView + zoomInput * cameraZoomSpeed * Time.deltaTime;
            topDownCamera.Lens.FieldOfView = Mathf.Clamp(zoomAmount, cameraZoomMin, cameraZoomMax);

            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(cameraTarget.transform.position + Vector3.up * 3, 1f);
    }

    private void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Vector2 coord = HexMetrics.CoordinateToOffset(hit.point.x, hit.point.z, grid.hexSize, grid.orientation);
            HexCell currentCell = grid.GetTile(coord);
            if (currentCell != null)
            {
                if(currentCell != outlinedCell) 
                {
                    outlinedCell = currentCell;

                    selectionOutline.SetActive(true);
                    selectionOutline.transform.position = new Vector3(
                        currentCell.terrain.position.x,
                        currentCell.terrainHigh + 0.001f,
                        currentCell.terrain.position.z
                    );

                    selectionOutline.transform.rotation = grid.orientation == HexOrientation.FlatTop
                        ? Quaternion.Euler(-90f, 30f, 0f)
                        : Quaternion.Euler(-90f, 0f, 0f);
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
    }
}

public enum CameraMode
{
    TopDown,
    Focus
}
