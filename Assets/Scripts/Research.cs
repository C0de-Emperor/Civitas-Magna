using UnityEngine;

[CreateAssetMenu(fileName = "Research", menuName = "Scriptable Objects/Research")]
public class Research : ScriptableObject
{
    public Sprite icon;
    public string researchName;
    [TextArea]
    public string description;
    public int scienceCost;

    public Research[] dependencies;
}
