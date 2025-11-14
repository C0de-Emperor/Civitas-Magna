using System;
using UnityEngine;

[System.Serializable]
public class Player
{
    public string playerName = "player";
    public Color[] livery = new Color[2];

    public Player(string playerName, Color[] livery )
    {
        this.playerName = playerName;
        this.livery = livery;
    }
}
