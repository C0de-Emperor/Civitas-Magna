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
    private GameObject cameraTarget;
    [SerializeField]
    private CinemachineCamera topDownCamera;
    [SerializeField]
    private CinemachineCamera focusCamera;
    [SerializeField]
    private CameraMode defaultMode = CameraMode.TopDown;
    [SerializeField]
    private CameraMode currentMode;

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
        topDownCamera.Lens.FieldOfView = cameraZoomDefault;
        ChangeCamera(defaultMode);
    }

    public void ChangeCamera(CameraMode mode)
    {
        if (!MapGenerator.instance.isMapReady)
            return;

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
        if (!MapGenerator.instance.isMapReady)
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
        ChangeCamera(CameraMode.TopDown);
    }

    public void OnZoomChanged(InputAction.CallbackContext context)
    {
        if (!MapGenerator.instance.isMapReady)
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
        if (!MapGenerator.instance.isMapReady)
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
        CinemachineFollow composer = topDownCamera.GetComponent<CinemachineFollow>();
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
                
                Vector3 offset = composer.FollowOffset;

                // Calcule la direction de l'offset (vers la caméra depuis la cible)
                Vector3 direction = offset.normalized;

                // Modifie la distance le long de cette direction
                float currentDistance = offset.magnitude;
                float newDistance = Mathf.Clamp(currentDistance + zoomInput * cameraZoomSpeed * Time.deltaTime, cameraZoomMin, cameraZoomMax);

                // Applique le nouvel offset
                composer.FollowOffset = direction * newDistance;

                // Modifie la vitesse de déplacement
                cameraSpeed = newDistance * 0.8f + 5f;
            }

            yield return null;
        }
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(cameraTarget.transform.position + Vector3.up * 3, 0.5f);
    }
}

public enum CameraMode
{
    TopDown,
    Focus
}
