using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager instance;

    private void Awake()
    {
        instance = this;
    }

    public static void ToggleUi(bool state)
    {
        PlayerLook.disableLook = state;
        Cursor.visible = state;
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
