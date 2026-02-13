using UnityEngine;

public class PopupTab : MonoBehaviour
{
    private void Awake()
    {
        UiManager.ToggleUi(true, true);
    }

    public void Close()
    {
        UiManager.ToggleUi(false, true);
        UiManager.CloseUiTabs();

        Destroy(gameObject);
    }
}
