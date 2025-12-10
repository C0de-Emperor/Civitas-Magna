using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class ProductionButton : MonoBehaviour
{
    [Header("Parameters")]
    [HideInInspector] public CityProductionItem item;

    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private Image image;
    [SerializeField] private Text prodNameText;
    [SerializeField] private Text prodTimeText;
    [SerializeField] private Image prodIcon;
    [SerializeField] private Transform bonusPanel;

    public void Init(CityProductionItem _item)
    {
        if (_item == null || button == null || image == null || prodNameText == null || prodTimeText == null || prodIcon == null)
        {
            Debug.LogWarning($"ProductionButton sur {gameObject.name} : référence manquante.");
            return;
        }

        item = _item;

        prodIcon.sprite = item.icon;
        prodNameText.text = item.itemName;
        button.onClick.AddListener(() => OnButtonClick());
    }

    private void Start()
    {
        GenerateBonuses();
    }

    private void GenerateBonuses()
    {
        if (item is not BuildingProductionItem building)
            return;

        var manager = BuildButtonManager.instance;

        (float value, Sprite icon)[] bonuses =
        {
        (building.bonusFood, manager.food),
        (building.bonusProduction, manager.prod),
        (building.bonusGold, manager.gold),
        (building.bonusScience, manager.science),
        (building.bonusHealth, manager.health),
    };

        foreach (var (value, icon) in bonuses)
        {
            if (value > 0)
                CreatePanel(value, icon);
        }
    }

    private void CreatePanel(float amount, Sprite sprite)
    {
        GameObject obj = Instantiate(BuildButtonManager.instance.bonusPrefab.gameObject, bonusPanel);

        obj.GetComponentInChildren<Text>().text = amount.ToString();
        obj.GetComponentInChildren<Image>().sprite = sprite;
    }

    private void OnButtonClick()
    {
        if (item is BuildingProductionItem building)
        {
            if (!CityManager.instance.openedCity.builtBuildings.Contains(building))
            {
                CityManager.instance.openedCity.SetProduction(item);
                BuildButtonManager.instance.RefreshUI(true);
            }
        }
        if (item is UnitProductionItem unit)
        {
            CityManager.instance.openedCity.SetProduction(item);
            BuildButtonManager.instance.RefreshUI(false);
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
                prodTimeText.text = "-";
                return;
            }
        }

        // -- Temps de production --
        int turns = Mathf.Max(1, CityManager.instance.GetTurnsToProduce(item, city));
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



}