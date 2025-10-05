using System;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    public Action<RaycastHit> OnLeftMouseClick;
    public Action<RaycastHit> OnRightMouseClick;
    public Action<RaycastHit> OnMiddleMouseClick;

    public static MouseController instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de MouseController dans la scène");
            return;
        }
        instance = this;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckMouseClick(0);
        }
        if (Input.GetMouseButtonDown(1))
        {
            CheckMouseClick(1);
        }
        if (Input.GetMouseButtonDown(2))
        {
            CheckMouseClick(2);
        }
    }

    private void CheckMouseClick(int mouseButton)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            switch (mouseButton)
            {
                case 0:
                    OnLeftMouseClick?.Invoke(hit); 
                    break;
                case 1:
                    OnRightMouseClick?.Invoke(hit);
                    break;
                case 2:
                    OnMiddleMouseClick?.Invoke(hit);
                    break;
            }
        }
    }
}
