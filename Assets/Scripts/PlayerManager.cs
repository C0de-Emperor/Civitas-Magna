using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [Header("Player")]
    public Player player = new Player("bruh", new Color[]{new Color(0,0,0),new Color(0,0,0)});

    [Header("UI")]
    [SerializeField] private Text goldStockText;

    [HideInInspector] public float goldStock = 0f;

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
        goldStockText.text = Mathf.RoundToInt(goldStock).ToString();
    }
}
