using UnityEngine;
using UnityEngine.Events;

public class UiTab : MonoBehaviour
{
    public UnityEvent Event;
    public bool destoryOnClose = false;

    public void CloseTab()
    {
        Event?.Invoke();

        if(destoryOnClose) Destroy(gameObject);
    }
}