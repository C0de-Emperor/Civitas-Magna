using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de SelectionManager dans la scène");
            return;
        }
        instance = this;
    }
}
