using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text goldStockText;

    [HideInInspector] public int goldStock = 0;

    public static PlayerManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de PlayerManager dans la scène");
            return;
        }
        instance = this;

        UpdateMainUI();
    }

    private void Start()
    {
        TurnManager.instance.OnTurnChange += UpdateMainUI;
    }

    private void UpdateMainUI()
    {
        goldStockText.text = goldStock.ToString();
    }




}
