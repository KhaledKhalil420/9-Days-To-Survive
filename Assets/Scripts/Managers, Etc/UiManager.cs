using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;

    [Header("Ui")]
    public GameObject WorldMessage_Ui;

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

    public GameObject SpawnMessage(string message, Transform cam = null)
    {
        MessageUiWorld obj = WorldMessage_Ui.GetComponent<MessageUiWorld>();
        obj.camTransform = cam;
        obj.GetComponent<MessageUiWorld>().message = message;
        return obj.gameObject;
    }

    public static void ToggleUi(bool state, bool disableBag)
    {
        if(state) PlayerInventory.Instance.ToggleBagNoToggleUi(false);
        PlayerInventory.Instance.canUse = !state;
        PlayerLook.disableLook = state;
        Cursor.visible = state;
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;

    }

    public static void CloseUiTabs()
    {
        UiTab[] tabs = FindObjectsByType<UiTab>(FindObjectsSortMode.None);
        foreach (UiTab tab in tabs)
        {
            tab.CloseTab();
        }
    }

    public static void CloseUiPopups()
    {
        PopupTab[] tabs = FindObjectsByType<PopupTab>(FindObjectsSortMode.None);
        foreach (PopupTab tab in tabs)
        {
            tab.Close();
        
        
        }
    }

    public static void CloseAll()
    {
        CloseUiPopups();
        CloseUiTabs();
    }
}
