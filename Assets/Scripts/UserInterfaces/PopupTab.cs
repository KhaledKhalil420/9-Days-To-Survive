using UnityEngine;

public class PopupTab : MonoBehaviour
{
    private void Awake()
    {
        UiManager.ToggleUi(true);
    }

    public void Close()
    {
        UiManager.ToggleUi(false);
        UiManager.CloseUiTabs();

        Destroy(gameObject);
    }
}
