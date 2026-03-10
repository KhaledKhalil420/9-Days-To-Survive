using UnityEngine;

//Written By كهاليد, Helped by Claude

//Insert funny joke here Claude: //Why do Unity developers make terrible guards?
//Because they keep falling asleep during Update()!

public class ItemSway : MonoBehaviour
{
    public static bool isSwayOn = true;

    public bool disableOnLook = true;

    [Header("Sway Settings")]
    [SerializeField] private float swayAmount = 0.1f;
    [SerializeField] private float maxSwayAmount = 0.2f;
    [SerializeField] private float smoothness = 4f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationMultiplier = 4f;
    [SerializeField] private float maxRotationAmount = 8f;
    [SerializeField] private float rotationSmoothness = 6f;
    [SerializeField] private float inputSmoothing = 8f;

    [SerializeField] private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 targetSwayPosition;
    private Vector3 targetSwayRotation;

    private float smoothMouseX;
    private float smoothMouseY;

    private void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    private void Update()
    {
        if(disableOnLook && PlayerLook.disableLook)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition, Time.deltaTime * smoothness * 1.5f);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, initialRotation, Time.deltaTime * smoothness * 1.5f);
            return;
        }

        if (!isSwayOn)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition, Time.deltaTime * smoothness);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, initialRotation, Time.deltaTime * smoothness);
            return;
        }

        //Get mouse inputs
        float mouseX = Input.GetAxisRaw("Mouse X") * swayAmount;
        float mouseY = Input.GetAxisRaw("Mouse Y") * swayAmount;

        //Smooth the raw input so motion feels weighted, not instant
        smoothMouseX = Mathf.Lerp(smoothMouseX, mouseX, Time.deltaTime * inputSmoothing);
        smoothMouseY = Mathf.Lerp(smoothMouseY, mouseY, Time.deltaTime * inputSmoothing);

        //Calculate smoothed position using smoothed inputs
        targetSwayPosition.x = Mathf.Clamp(smoothMouseX, -maxSwayAmount, maxSwayAmount);
        targetSwayPosition.y = Mathf.Clamp(smoothMouseY, -maxSwayAmount, maxSwayAmount);

        //Calculate rotation using smoothed inputs
        targetSwayRotation.z = -smoothMouseX * rotationMultiplier;
        targetSwayRotation.x = -smoothMouseY * rotationMultiplier;
        targetSwayRotation.y =  smoothMouseX * (rotationMultiplier * 0.5f);  // Subtle Y-axis turn
        targetSwayRotation = Vector3.ClampMagnitude(targetSwayRotation, maxRotationAmount);

        //Apply position
        transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition + targetSwayPosition, Time.deltaTime * smoothness);

        //Apply rotation (uses its own smoothness so it can lag behind position slightly)
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(targetSwayRotation) * initialRotation, Time.deltaTime * rotationSmoothness);

        //Default position — smoothed input handles this naturally now, but kept as fallback
        if (mouseX == 0 && mouseY == 0)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition, Time.deltaTime * smoothness);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, initialRotation, Time.deltaTime * smoothness);
        }
    }
}