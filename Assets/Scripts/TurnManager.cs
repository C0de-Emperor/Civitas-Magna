using System;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public int currentTurn = 0;

    // toute fonction ajouté à cet event sera executé au changement de tour
    public event Action OnTurnChange;

    public static TurnManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de TurnManager dans la scène");
            return;
        }
        instance = this;

        OnTurnChange += Log;
    }

    public void ChangeTurn()
    {
        currentTurn += 1;

        OnTurnChange?.Invoke();
    }

    private void Log()
    {
        Debug.Log("Turn is now : " + currentTurn.ToString());
    }
}
