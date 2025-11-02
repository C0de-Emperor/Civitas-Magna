using Unity.Mathematics;
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
    [HideInInspector] public Vector3 offset;

    [Header("Scale Settings")]
    public float minScale = 0.6f;    // à grande distance
    public float maxScale = 1.2f;    // proche caméra
    public float minDistance = 15f;  // distance de la caméra où le scale est max
    public float maxDistance = 80f;  // distance où le scale est min
    public bool hideWhenTooFar = false;

    private Camera mainCamera;
    private RectTransform rect;

    private void Start()
    {
        mainCamera = Camera.main;
        rect = GetComponent<RectTransform>();

        offset = new Vector3(0, 2f, 0);
    }

    private void LateUpdate()
    {
        if (worldTarget == null || mainCamera == null) return;

        Vector3 worldPos = worldTarget.position + offset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        if (screenPos.z > 0)
        {
            transform.position = screenPos;

            // Calcul de la distance pour adapter le scale
            float distance = Vector3.Distance(mainCamera.transform.position, worldPos);
            float t = Mathf.InverseLerp(minDistance, maxDistance, distance);
            float scale = Mathf.Lerp(maxScale, minScale, t);

            rect.localScale = Vector3.one * scale;

            if (hideWhenTooFar)
                gameObject.SetActive(distance < maxDistance * 1.2f);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void UpdateHealth(float health, float maxHealth)
    {
        float healthPercentage = health / maxHealth;
        healthBar.fillAmount = (float)math.remap(0, 1, 0, 0.25, healthPercentage);

        healthBar.color = gradient.Evaluate(healthPercentage);
    }

    public void SetPinColour(Color[] livery)
    {
        PinBackground.color=livery[0];
        UnitSprite.color=livery[1];
        UpdateHealth(1, 1);
    }

    public void InitializePin(Sprite unitSprite, Color[] livery)
    {
        this.PinBackground.color = livery[0];

        this.UnitSprite.sprite = unitSprite;
        this.UnitSprite.SetNativeSize();
        this.UnitSprite.color = livery[1];

        UpdateHealth(1, 1);
    }
}
