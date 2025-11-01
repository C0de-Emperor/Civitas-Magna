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
    [HideInInspector] public Button button;
    [HideInInspector] public Image image;
    [HideInInspector] public Text prodNameText;
    [HideInInspector] public Text prodTimeText;
    [HideInInspector] public Image prodIcon;

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();

        prodNameText = transform.Find("Name").GetComponent<Text>();
        prodTimeText = transform.Find("Turn").GetComponent<Text>();
        prodIcon = transform.Find("Icon").GetComponent<Image>();

        if (item == null || button == null || image == null || prodNameText == null || prodTimeText == null || prodIcon == null)
        {
            Debug.LogWarning($"ProductionButton sur {gameObject.name} : référence manquante.");
            return;
        }

        prodIcon.sprite = item.icon;
        prodNameText.text = item.itemName;
        button.onClick.AddListener(() => OnButtonClick());
    }

    private void OnButtonClick()
    {
        if (item is BuildingProductionItem building)
        {
            if (!CityManager.instance.openedCity.builtBuildings.Contains(building))
            {
                CityManager.instance.openedCity.SetProduction(item);
                BuildButtonManager.instance.RefreshUI();
            }
        }
    }

    public void UpdateVisual(Sprite selectedSprite, Color activeColor, Sprite unselectedSprite, Color unactiveColor, Color builtColor)
    {
        City city = CityManager.instance.openedCity;
        if (city == null)
            return;

        if(item is  BuildingProductionItem building)
        {
            if (city.builtBuildings.Contains(building))
            {
                image.sprite = unselectedSprite;
                image.color = builtColor;
                return;
            }
        }
        
        // -- Temps de production --
        int turns = Mathf.Max(1, GetTurnsToProduce());
        prodTimeText.text = turns.ToString();

        // -- Apparence du bouton --
        if (city.currentProduction == item) 
        { 
            image.color = activeColor; 
            button.interactable = false; 
            image.sprite = selectedSprite; 
        } 
        else 
        { 
            image.color = unactiveColor; 
            button.interactable = true; 
            image.sprite = unselectedSprite; 
        }
    }


    private int GetTurnsToProduce()
    {
        float prodRequired = item.costInProduction - CityManager.instance.openedCity.currentProductionProgress;
        float net = CityManager.instance.openedCity.GetCityProduction();

        return Mathf.CeilToInt(prodRequired / net);

    }

}