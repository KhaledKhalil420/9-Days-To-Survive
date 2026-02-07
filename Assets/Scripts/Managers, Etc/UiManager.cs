using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public static void ToggleUi(bool state)
    {
        PlayerLook.disableLook = state;
        Cursor.visible = state;
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
