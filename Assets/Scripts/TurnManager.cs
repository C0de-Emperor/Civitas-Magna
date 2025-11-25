using System;
using System.Collections;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public int currentTurn;
    private bool canChangeTurn;

    // toute fonction ajouté à cet event sera executé au changement de tour
    public event Action OnTurnChange;

    public static TurnManager instance;
    private void Awake()
    {
        canChangeTurn = false;

        SaveManager.instance.OnSaveLoaded += OnLoad;
        SaveManager.instance.OnNewGameStarted += OnStartNewGame;
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de TurnManager dans la scène");
            return;
        }
        instance = this;

        OnTurnChange += Log;

        StartCoroutine(ResetChangeTurn());
    }

    private void OnLoad(SaveData data)
    {
        if(data == null)
            throw new Exception("SaveData is null");

        currentTurn = data.currentTurn;     
    }

    private void OnStartNewGame(NewGameData data)
    {
        currentTurn = 0;
    }

    public void ChangeTurn()
    {
        if (!canChangeTurn || UnitManager.instance.movingUnitsCount > 0)
            return;

        canChangeTurn = false;
        currentTurn += 1;

        OnTurnChange?.Invoke();

        StartCoroutine(ResetChangeTurn());
    }

    private IEnumerator ResetChangeTurn()
    {
        yield return null;

        canChangeTurn = true;
    }

    private void Log()
    {
        Debug.Log("Turn is now : " + currentTurn.ToString());
    }
}
