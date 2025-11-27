using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Research", menuName = "Scriptable Objects/Research")]
public class Research : ScriptableObject
{
    public Sprite icon;
    public string researchName;
    [TextArea]
    public string description;
    public int scienceCost;

    [Header("UI")]
    public int index;
    public int depth;

    public Dependency[] dependencies;  
    public Unlock[] unlocks;

    [Header("AI")]
    public float researchValue
    {
        get
        {
            return depth + scienceCost/(10*(depth != 0 ? depth : 1)) + dependencies.Length + unlocks.Length;
        }
        private set { }
    }
}

[Serializable]
public class Dependency
{
    public Research research;
    public float dependencyLineDepth;
}

[Serializable]
public class Unlock
{
    public Sprite icon;
    public enum UnlockType { Building, Unit, Amenagement }
    public UnlockType type;
}