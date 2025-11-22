using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class UnitPin : MonoBehaviour
{
    public Image PinBackground;
    public Image UnitSprite;
    public Image healthBar;

    public Gradient gradient;

    [HideInInspector] public Transform worldTarget;
    public Vector3 offset;

    [Header("Scale Settings")]
    public float minScale = 0.7f;
    public float maxScale = 0.1f;
    public float minDistance = 15f;
    public float maxDistance = 80f;
    public bool hideWhenTooFar = false;

    private Camera mainCamera;
    private RectTransform rect;

    private void Awake()
    {
        mainCamera = Camera.main;
        rect = GetComponent<RectTransform>();

        offset = new Vector3(0, 1.5f, 0);

        minDistance = CameraController.instance.cameraZoomMin;
        maxDistance = CameraController.instance.cameraZoomMax;
    }

    private void LateUpdate()
    {
        if (worldTarget == null || mainCamera == null) return;

        Vector3 worldPos = worldTarget.position + offset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        transform.position = screenPos;

        float distance = Vector3.Distance(mainCamera.transform.position, worldPos);

        /*
        // Calcul de la distance pour adapter le scale
        float distance = Vector3.Distance(mainCamera.transform.position, worldPos);
        float t = Mathf.InverseLerp(minDistance, maxDistance, distance);
        float scale = Mathf.Lerp(maxScale, minScale, t);

        rect.localScale = Vector3.one * scale;*/

        if (hideWhenTooFar)
            gameObject.SetActive(distance < maxDistance * 1.2f);
    }

    public void UpdateScale(float newDistance)
    {
        rect.localScale = Vector3.one * math.remap(minDistance, maxDistance, minScale, maxScale, newDistance);
    }

    public void UpdateHealth(float health, float maxHealth)
    {
        float healthPercentage = health / maxHealth;
        healthBar.fillAmount = (float)math.remap(0, 1, 0, 0.25, healthPercentage);

        healthBar.color = gradient.Evaluate(healthPercentage);
    }

    public void InitializePin(Sprite unitSprite, Livery livery, float lastDistance)
    {
        this.PinBackground.color = livery.backgroundColor;

        this.UnitSprite.sprite = unitSprite;
        this.UnitSprite.color = livery.spriteColor;

        UpdateHealth(1, 1);
        UpdateScale(lastDistance);
    }
}
