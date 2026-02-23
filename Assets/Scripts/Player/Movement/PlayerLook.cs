using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public static PlayerLook instance;

    public float Sensitivity = 50f;
    [SerializeField] internal Vector2 rotations;
    internal Vector3 offset;

    public Transform Player;
    private CapsuleCollider playerCollider;

    internal static bool disableLook = false;

    public static Camera mainCamera;
    public Camera _mainCamera;
    [SerializeField] private LayerMask unRenderableLayers;

    public void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        instance = this;

        rotations.y = transform.localEulerAngles.y;
        playerCollider = Player.GetComponent<CapsuleCollider>();

        _mainCamera.enabled = true;
        _mainCamera.tag = "MainCamera";
        _mainCamera.cullingMask = ~unRenderableLayers;

        mainCamera = _mainCamera;

        transform.parent = null;
    }

    public void Update()
    {        
        Look();
        AutoFix();

        if(!disableLook)
        {
            Inputs();
        }
    }

    public void LateUpdate()
    {
        Follow();
    }

    private void AutoFix()
    {
        if(mainCamera == null) 
            mainCamera = _mainCamera;
    }

    private void Inputs()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Sensitivity * 0.01f;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Sensitivity * 0.01f;

        rotations.x -= mouseY;
        rotations.y += mouseX;

        rotations.x = Mathf.Clamp(rotations.x, -90f, 90f);
    }

    private void Look()
    {
        if (Player == null) return;

        Player.rotation = Quaternion.Euler(0f, rotations.y, 0f);
        transform.rotation = Quaternion.Euler(rotations.x, rotations.y, 0f);
    }

    private void Follow()
    {
        if (playerCollider == null) return;

        transform.position = playerCollider.bounds.center + new Vector3(0, playerCollider.bounds.extents.y - 0.25f, 0) + offset;
    }

    public Vector3 ForwardDirection() => transform.forward;
}