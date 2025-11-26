using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float Sensitivity = 50f;
    [SerializeField] internal Vector2 rotations;
    internal Vector3 offset;

    public Transform Player;
    private CapsuleCollider playerCollider;

    internal bool disableLook;

    public static Camera mainCamera;
    public Camera _mainCamera;

    public void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rotations.y = transform.localEulerAngles.y;
        playerCollider = Player.GetComponent<CapsuleCollider>();

        _mainCamera.enabled = true;
        _mainCamera.tag = "MainCamera";

        mainCamera = _mainCamera;

        transform.parent = null;
    }

    public void Update()
    {
        Inputs();
        Look();
    }

    public void LateUpdate()
    {
        Follow();
    }

    private void Inputs()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Sensitivity * 0.1f;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Sensitivity * 0.1f;

        rotations.x -= mouseY;
        rotations.y += mouseX;

        rotations.x = Mathf.Clamp(rotations.x, -90f, 90f);
    }

    private void Look()
    {
        if (Player == null) return;

        Player.eulerAngles = new Vector3(Player.eulerAngles.x, rotations.y, Player.eulerAngles.z);
        transform.eulerAngles = new Vector3(rotations.x, Player.eulerAngles.y, transform.eulerAngles.z);
    }

    private void Follow()
    {
        if (playerCollider == null) return;

        transform.position = playerCollider.bounds.center + new Vector3(0, playerCollider.bounds.extents.y - 0.25f, 0) + offset;
    }

    public Vector3 ForwardDirection() => transform.forward;
}
