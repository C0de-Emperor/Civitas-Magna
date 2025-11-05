using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CityBannerUI : MonoBehaviour
{
    public Text cityNameText;
    public Text levelText;
    public Text turnText;
    public Slider healthBar;
    public Image currentProd;
    public Text currentProdTurn;

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

        offset = new Vector3(0, 2.5f, 0);
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

    public void UpdateInfo(string name, int level, int turns, float damage, float maxHealth, Sprite prodIcon, int prodturn)
    {
        cityNameText.text = name;
        levelText.text = level.ToString();

        if (turns < 0)
        {
            turnText.color = Color.red;
            turnText.text = (-turns).ToString();
        }
        else if (turns > 0)
        {
            turnText.color = Color.black;
            turnText.text = turns.ToString();
        }
        else
        {
            turnText.color = Color.black;
            turnText.text = "--";
        }

        healthBar.maxValue = maxHealth;
        healthBar.minValue = 0f;

        healthBar.value = maxHealth - damage;

        if(prodIcon != null)
        {
            currentProd.gameObject.SetActive(true);
            currentProd.sprite = prodIcon;
        }
        else
        {
            currentProd.gameObject.SetActive(false);
        }

        if(prodturn > 0)
            currentProdTurn.text = prodturn.ToString();
        else
            currentProdTurn.text = "";
    }
}
