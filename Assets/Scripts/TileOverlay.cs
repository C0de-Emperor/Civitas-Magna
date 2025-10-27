using UnityEngine;
using UnityEngine.UI;

public class TileOverlay : MonoBehaviour
{
    [SerializeField] private Text foodText;
    [SerializeField] private Text prodText;
    //private Camera mainCam;

    private void Awake()
    {
        //mainCam = Camera.main;
    }

    public void Init(int food, int prod)
    {
        foodText.text = food.ToString();
        prodText.text = prod.ToString();
    }

    private void LateUpdate()
    {
        // Face caméra
        //if (mainCam != null)
            //transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
    }
}
