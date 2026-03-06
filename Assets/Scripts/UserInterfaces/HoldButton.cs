using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Settings")]
    [SerializeField] private float holdDuration = 2f;

    [Header("UI")]
    [SerializeField] private Image fillImage;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float minPitch = 0.25f;
    [SerializeField] private float maxPitch = 1f;

    [Header("Events")]
    [SerializeField] private UnityEvent onComplete;

    private bool isHolding = false;
    private float holdProgress = 0f;

    private void Update()
    {
        if (isHolding)
        {
            holdProgress += Time.deltaTime / holdDuration;
            fillImage.fillAmount = holdProgress;
            audioSource.volume = holdProgress;
            audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, holdProgress);

            if (holdProgress >= 1f)
            {
                isHolding = false;
                AudioManager.Instance.PlaySound("Ui_Click");
                onComplete?.Invoke();
                ResetButton();
            }
        }
        else if (holdProgress > 0f)
        {
            holdProgress -= Time.deltaTime / holdDuration;
            holdProgress = Mathf.Max(0f, holdProgress);
            fillImage.fillAmount = holdProgress;
            audioSource.volume = holdProgress;
            audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, holdProgress);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        audioSource.Play();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
    }

    private void ResetButton()
    {
        holdProgress = 0f;
        fillImage.fillAmount = 0f;
        audioSource.volume = 0f;
        audioSource.pitch = minPitch;
        audioSource.Stop();
    }
}