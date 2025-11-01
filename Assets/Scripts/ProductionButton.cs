using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class ProductionButton : MonoBehaviour
{
    [Header("Parameters")]
    public CityProductionItem item;

    [Header("References")]
    private Button button;
    private Image image;

    public Text prodNameText;
    public Text prodTimeText;
    public Image prodIcon;

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

        button.onClick.AddListener(() => OnButtonClick());

        image.color = unactiveColor;
        image.sprite = CityManager.instance.unselectedProd;

        prodIcon.sprite = item.icon;
        prodNameText.text = item.itemName;
    }

    private void OnButtonClick()
    {
        if (item is BuildingProductionItem building)
        {
            if (!CityManager.instance.openedCity.builtBuildings.Contains(building))
            {
                CityManager.instance.openedCity.SetProduction(item);
                UpdateVisual();
            }
        }
    }

    private void UpdateVisual()
    {
        prodTimeText.text = GetTurnsToProduce().ToString();
        Debug.Log(GetTurnsToProduce().ToString());

        if (CityManager.instance.openedCity.currentProduction == item)
        {
            image.color = activeColor;
            button.interactable = false;
            image.sprite = CityManager.instance.selectedProd;
        }
        else
        {
            image.color = unactiveColor;
            button.interactable = true;
            image.sprite = CityManager.instance.unselectedProd;
        }
    }

    private int GetTurnsToProduce()
    {
        float prodRequired = item.costInProduction - CityManager.instance.openedCity.currentProductionProgress;
        float net = CityManager.instance.openedCity.GetCityProduction();

        return Mathf.CeilToInt(prodRequired / net);

    }

    private void OnEnable()
    {
        if (CityManager.instance == null || CityManager.instance.openedCity == null)
        {
            return;
        }
            

       
        if (item is BuildingProductionItem building)
        {
            if (CityManager.instance.openedCity.builtBuildings.Contains(building))
            {
                prodTimeText.text = "";
                image.color = builtColor;
                button.interactable = false;
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