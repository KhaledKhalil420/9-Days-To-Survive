using UnityEngine;

//Written By كهاليد, Helped by Claude

//Insert funny joke here Claude: //Why do Unity developers make terrible guards?
//Because they keep falling asleep during Update()!

public class ItemSway : MonoBehaviour
{
    public static bool isSwayOn = true;

    [Header("Sway Settings")]
    [SerializeField] private float swayAmount = 0.1f;
    [SerializeField] private float maxSwayAmount = 0.2f;
    [SerializeField] private float smoothness = 4f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationMultiplier = 1f;
    [SerializeField] private float maxRotationAmount = 5f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 targetSwayPosition;
    private Vector3 targetSwayRotation;

    private void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    private void Update()
    {
        if (!isSwayOn)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition, Time.deltaTime * smoothness);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, initialRotation, Time.deltaTime * smoothness);
            return;
        }

        //Get mouse inputs
        float mouseX = Input.GetAxisRaw("Mouse X") * swayAmount;
        float mouseY = Input.GetAxisRaw("Mouse Y") * swayAmount;

        //Calculate smoothed position
        targetSwayPosition.x = Mathf.Clamp(mouseX, -maxSwayAmount, maxSwayAmount);
        targetSwayPosition.y = Mathf.Clamp(mouseY, -maxSwayAmount, maxSwayAmount);

        //Calculate smoothed rotation
        targetSwayRotation.z = -targetSwayPosition.x * rotationMultiplier;
        targetSwayRotation.x = -targetSwayPosition.y * rotationMultiplier;
        targetSwayRotation = Vector3.ClampMagnitude(targetSwayRotation, maxRotationAmount);

        //Apply position
        transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition + targetSwayPosition, Time.deltaTime * smoothness);

        //Apply rotation
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(targetSwayRotation) * initialRotation, Time.deltaTime * smoothness);

        //Default position
        if (mouseX == 0 && mouseY == 0)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition, Time.deltaTime * smoothness);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, initialRotation, Time.deltaTime * smoothness);
        }
    }
}
