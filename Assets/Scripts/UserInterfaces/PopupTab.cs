using UnityEngine;

public class PopupTab : MonoBehaviour
{
    public void Close()
    {
        UiManager.ToggleUi(false);
        Destroy(gameObject);
    }
}
