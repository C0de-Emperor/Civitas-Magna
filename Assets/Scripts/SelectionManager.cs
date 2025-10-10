using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de SelectionManager dans la sc�ne");
            return;
        }
        instance = this;
    }
}
