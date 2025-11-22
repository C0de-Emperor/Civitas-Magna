using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;


public class CameraController : MonoBehaviour
{
    public bool canMove = true;

    [SerializeField]
    private const int DEFAULT_PRIORITY = 10;

    [Header("References")]
    [SerializeField]
    private HexGrid grid;
    [SerializeField]
    private GameObject cameraTarget;
    [SerializeField]
    private CinemachineCamera defaultCamera;
    [SerializeField]
    private CinemachineCamera focusCamera;
    [SerializeField]
    private CinemachineCamera topCityFocusCamera;
    [SerializeField]
    private CameraMode defaultMode = CameraMode.Default;
    [SerializeField]
    private CameraMode currentMode;

    [Header("Parameters")]
    [SerializeField] private float cameraSpeed = 10f;
    [SerializeField] private float cameraZoomSpeed = 1f;
    [SerializeField] public float cameraZoomMin = 15f;
    [SerializeField] public float cameraZoomMax = 150f;
    [SerializeField] private float cameraZoomDefault = 50f;

    private float mapMinX;
    private float mapMaxX;
    private float mapMinZ;
    private float mapMaxZ;

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
        defaultCamera.Lens.FieldOfView = cameraZoomDefault;
        ChangeCamera(defaultMode);

        mapMinX = 0f;
        mapMaxX = grid.width * grid.hexSize * ((grid.orientation == HexOrientation.FlatTop)? 1.5f : 1.75f);
        mapMinZ = 0f;
        mapMaxZ = grid.height * grid.hexSize * ((grid.orientation == HexOrientation.FlatTop) ? 1.75f : 1.5f);

        ApplyZoom(0f, defaultCamera.GetComponent<CinemachineFollow>());
    }

    public void SetTargetPosition(Vector2 pos)
    {
        cameraTarget.transform.position = new Vector3(pos.x, cameraTarget.transform.position.y, pos.y);
    }

    public void ChangeCamera(CameraMode mode)
    {
        if (currentMode == mode) return;

        currentMode = mode;
        CinemachineCamera cam = GetCamera(mode);

        if (cam == null)
        {
            Debug.LogError($"No Cinemachine camera assigned for mode: {mode}");
            return;
        }

        onCameraChanged?.Invoke(cam);

        // baisse la priorité des autres caméras
        ResetAllCameraPriorities();
        cam.Priority = DEFAULT_PRIORITY;
    }

    private CinemachineCamera GetCamera(CameraMode mode)
    {
        return mode switch
        {
            CameraMode.Default => defaultCamera,
            CameraMode.Focus => focusCamera,
            CameraMode.TopFocusCity => topCityFocusCamera,
            _ => null
        };
    }

    private void ResetAllCameraPriorities()
    {
        defaultCamera.Priority = 0;
        //focusCamera.Priority = 0;
        topCityFocusCamera.Priority = 0;
    }

    public void OnPanChange(InputAction.CallbackContext context)
    {
        if (!MapGenerator.instance.isMapReady || !canMove)
            return;

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
        //ChangeCamera(CameraMode.Default);
    }

    public void OnZoomChanged(InputAction.CallbackContext context)
    {
        if (!MapGenerator.instance.isMapReady || !canMove)
            return;

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
        if (!MapGenerator.instance.isMapReady || !canMove)
            return;

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
            // Lire le mouvement
            Vector2 inputVector = context.ReadValue<Vector2>();
            Vector3 moveVector = new Vector3(inputVector.x, 0, inputVector.y);

            // Déplacer la caméra
            cameraTarget.transform.position += moveVector * cameraSpeed * Time.deltaTime;

            // Récupérer la position actuelle
            Vector3 pos = cameraTarget.transform.position;

            // Restreindre aux bornes de la carte
            pos.x = Mathf.Clamp(pos.x, mapMinX, mapMaxX);
            pos.z = Mathf.Clamp(pos.z, mapMinZ, mapMaxZ);

            // Appliquer la position corrigée
            cameraTarget.transform.position = pos;

            yield return null;
        }
    }

    public IEnumerator ProcessZoom(InputAction.CallbackContext context)
    {
        CinemachineFollow composer = defaultCamera.GetComponent<CinemachineFollow>();
        if(composer == null)
        {
            Debug.Log("Pas de CinemachineFollow");
            yield return null;
        }

        while (true)
        {
            float zoomInput = context.ReadValue<float>();
            if (zoomInput != 0)
            {
                ApplyZoom(zoomInput, composer);
            }

            yield return null;
        }
    }

    private void ApplyZoom(float zoomInput, CinemachineFollow composer)
    {
        Vector3 offset = composer.FollowOffset;

        // Calcule la direction de l'offset (vers la caméra depuis la cible)
        Vector3 direction = offset.normalized;

        // Modifie la distance le long de cette direction
        float currentDistance = offset.magnitude;
        float newDistance = Mathf.Clamp(currentDistance + zoomInput * cameraZoomSpeed * Time.deltaTime, cameraZoomMin, cameraZoomMax);

        // Applique le nouvel offset
        composer.FollowOffset = direction * newDistance;

        // Modifie la vitesse de déplacement
        cameraSpeed = newDistance * 0.8f;

        UnitManager.instance.UpdatePinsScale(newDistance);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(cameraTarget.transform.position + Vector3.up * 3, 0.25f);
    }
}

public enum CameraMode
{
    Default,
    Focus,
    TopFocusCity
}
