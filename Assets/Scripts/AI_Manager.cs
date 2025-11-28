using System;
using UnityEngine;

public class AI_Manager : MonoBehaviour
{
    private void Start()
    {
        TurnManager.instance.OnTurnChange += DoActions;
    }

    private void DoActions()
    {

        /*
        Get All Informations
        -------------
        research power
        combat power
        expantion
        amenagement

        -------------

        Start Research

        Set All City Production

        Move All Unit
        */
    }
}
