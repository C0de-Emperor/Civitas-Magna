using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class ProductionButton : MonoBehaviour
{
    [Header("Parameters")]
    private Button button;
    private Image image;
    public CityProductionItem item;

    private Color activeColor = new Color(1, 1, 1, 1);
    private Color unactiveColor = new Color(0.8f, 0.8f, 0.8f, 1);
    private Color builtColor = new Color(0.5f, 1f, 0.5f, 1);

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
    }

    private void Start()
    {
        if (CityManager.instance.openedCity == null)
        {
            Debug.LogWarning("No opened city.");
            return;
        }
        if (item == null)
        {
            Debug.LogWarning($"ProductionButton sur {gameObject.name} : référence manquante.");
            return;
        }

        button.onClick.AddListener(() => CityManager.instance.openedCity.SetProduction(item));
        button.onClick.AddListener(() => UpdateVisual());

        image.color = unactiveColor;
        image.sprite = CityManager.instance.unselectedProd;
    }

    private void UpdateVisual()
    {
        if (CityManager.instance.openedCity.currentProduction == item)
        {
            image.color = activeColor;
            image.sprite = CityManager.instance.selectedProd;
        }
        else
        {
            image.color = unactiveColor;
            image.sprite = CityManager.instance.unselectedProd;
        }
    }

    private void OnEnable()
    {
        if (CityManager.instance == null || CityManager.instance.openedCity == null)
            return;

       
        if (item is BuildingProductionItem building)
        {
            //Debug.Log()
            if (CityManager.instance.openedCity.builtBuildings.Contains(building))
            {
                image.color = builtColor;
            }
            else
            {
                UpdateVisual();
            }
        }
        else
        {
            UpdateVisual();
        }
    }
}