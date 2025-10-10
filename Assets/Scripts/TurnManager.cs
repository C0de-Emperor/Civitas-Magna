using System;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public int currentTurn = 0;

    // toute fonction ajout� � cet event sera execut� au changement de tour
    public event Action OnTurnChange;

    public static TurnManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de TurnManager dans la sc�ne");
            return;
        }
        instance = this;

        OnTurnChange += O;
    }

    public void ChangeTurn()
    {
        currentTurn += 1;

        OnTurnChange?.Invoke();
    }

    private void O()
    {
        Debug.Log("Turn is now : " + currentTurn.ToString());
    }
}
