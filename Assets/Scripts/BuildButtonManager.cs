using UnityEngine;
using UnityEngine.UI;

public class BuildButtonManager : MonoBehaviour
{
    [Header("References")]
    public ProductionButton[] buildingButtons;
    public ProductionButton[] unitButtons;
    public RectTransform buildingContentPanel;
    public RectTransform unitContentPanel;
    public Transform bonusPrefab;

    [Header("Colors")]
    public Color activeColor = new Color(1, 1, 1, 1);
    public Color unactiveColor = new Color(0.8f, 0.8f, 0.8f, 1);
    public Color builtColor = new Color(0.5f, 1f, 0.5f, 1);

    [Header("Sprites")]
    public Sprite selectedSprite;
    public Sprite unselectedSprite;

    public Sprite food;
    public Sprite prod;
    public Sprite gold;
    public Sprite science;
    public Sprite health;

    public static BuildButtonManager instance; 
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de BuildButtonManager dans la scène");
            return;
        }
        instance = this;
    }

    public void RefreshUI(bool isBuildingMenu)
    {
        ProductionButton[] b = isBuildingMenu? buildingButtons : unitButtons;
        int showedButton = 0;

        City city = CityManager.instance.openedCity;
        if (city == null || b.Length == 0)
            return;

        foreach (ProductionButton button in b)
        {
            bool shouldBeVisible = true;

            // Condition a l'affichage d'un bouton : recherches / batiments requis
            if (button.item is BuildingProductionItem building)
            {
                foreach(BuildingProductionItem requirment in building.requierments)
                {
                    if (!city.builtBuildings.Contains(requirment))
                    {
                        shouldBeVisible = false;
                        break;
                    }
                }
            }
            button.gameObject.SetActive(shouldBeVisible);

            if (shouldBeVisible)
            {
                showedButton++;
                button.UpdateVisual(selectedSprite, activeColor, unselectedSprite, unactiveColor, builtColor);
            }

            // --- Ajustement de la taille du Content ---
            if (showedButton > 0)
            {
                float buttonHeight = b[0].GetComponent<RectTransform>().rect.height;
                float spacing = 5f; // ou la même valeur que ton VerticalLayoutGroup spacing
                float totalHeight = buttonHeight * showedButton + spacing * (showedButton - 1);

                RectTransform contentRect = (isBuildingMenu? buildingContentPanel : unitContentPanel).GetComponent<RectTransform>();
                contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
            }
        }
    }

}
