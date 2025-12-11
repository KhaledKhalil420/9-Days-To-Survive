using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour, IInteractable
{
    public enum DoorState { Closed, Open }

    [Header("Door Settings")]
    [SerializeField] private DoorState currentState = DoorState.Closed;

    [Header("Smooth Rotation")]
    [SerializeField] private float openAngle = 90f;    // z angle when open (will be + or -)
    [SerializeField] private float xAngle = -90f;      // fixed local x angle
    [SerializeField] private float rotateDuration = 0.25f;

    private Coroutine rotateRoutine;
    private bool isAnimating;

    public void Interact(GameObject sender)
    {
        if (isAnimating) return;

        if (currentState == DoorState.Closed)
        {
            // determine which side the sender is on in door's local space
            Vector3 localPos = transform.InverseTransformPoint(sender.transform.position);
            float targetZ = localPos.x >= 0f ? openAngle : -openAngle;
            StartRotation(targetZ);
            currentState = DoorState.Open;
        }
        else // Open -> Close
        {
            StartRotation(0f);
            currentState = DoorState.Closed;
        }
    }

    private void StartRotation(float targetZ)
    {
        if (rotateRoutine != null) StopCoroutine(rotateRoutine);
        Quaternion target = Quaternion.Euler(xAngle, 0f, targetZ);
        rotateRoutine = StartCoroutine(RotateTo(target, rotateDuration));
    }

    private IEnumerator RotateTo(Quaternion targetRot, float duration)
    {
        isAnimating = true;
        Quaternion start = transform.localRotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            transform.localRotation = Quaternion.Slerp(start, targetRot, t);
            yield return null;
        }

        transform.localRotation = targetRot;
        isAnimating = false;
    }
}
