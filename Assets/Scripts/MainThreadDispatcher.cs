using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private readonly Queue<System.Action> _executionQueue = new Queue<System.Action>();

    public static MainThreadDispatcher instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de MainThreadDispatcher dans la scène");
            return;
        }
        instance = this;
    }

    public void Enqueue(System.Action action)
    {
        // lock the queue while adding to avoid contention with other threads
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }

    private void Update()
    {
        // dispatch stuff on main thread and remove from queue
        while (_executionQueue.Count > 0)
        {
            _executionQueue.Dequeue().Invoke();
        }
    }

    public void Clear()
    {
        lock (_executionQueue)
        {
            _executionQueue.Clear();
        }
    }
}