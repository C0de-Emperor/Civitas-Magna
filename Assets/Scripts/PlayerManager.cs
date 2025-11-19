using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public int nextAvailableId = 0;
    public Player player;

    public List<Player> playerEntities = new List<Player>();

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

        SaveManager.instance.OnSaveLoaded += OnLoad;
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

    public void OnLoad(SaveData saveData)
    {
        if(saveData != null)
        {
            player = new Player(saveData.player.playerName, saveData.player.livery);
        }
        else
        {
            player = new Player("bruh", new Color[] { new Color(1, 1, 1), new Color(52f/255, 182f/255, 23f/255) });
        }
    }
}

[Serializable]
public class Player
{
    public int id;
    public string playerName = "player";
    public Color[] livery = new Color[2];

    public Player(string playerName, Color[] livery)
    {
        this.id = PlayerManager.instance.nextAvailableId;
        PlayerManager.instance.nextAvailableId++;

        this.playerName = playerName;
        this.livery = livery;
        
        PlayerManager.instance.playerEntities.Add(this);
    }
}