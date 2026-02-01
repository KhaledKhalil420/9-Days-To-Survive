using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private PlayerLook look;
    [SerializeField] private LayerMask highlightableLayers;
    [SerializeField] internal float raycastDistance;
    [SerializeField] private Transform crosshair;

    [Header("Hold To Interact")]
    [SerializeField] private Image holdProgress;
    [SerializeField] private float holdSpeed;

    private IHighlightable currentlyHighlightable;

    private void LateUpdate()
    {
        HandleHighLight();
        Inputs();
    }

    private void Inputs()
    {
        if (Input.GetKeyDown(Keybinds.Key("Interact")))
        {
            TryInteractOnce();
        }

        if (Input.GetKeyDown(Keybinds.Key("Interact")))
        {
            TryInteract();
        }
        else if (Input.GetKey(Keybinds.Key("Interact")))
        {
            TryInteractHold();
        }

        else if (Input.GetKeyUp(Keybinds.Key("Interact")))
        {
            StopHoldInteract();
        }

        if (Input.GetMouseButtonDown(2))
        {
            TryAltInteract();
         }
    }

    private void TryInteract()
    {
        if (Physics.Raycast(look.transform.position, look.transform.forward, out RaycastHit hit, raycastDistance, LayerMask.GetMask("Interactable", "Farmland")))
        {
            if (hit.transform.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact(gameObject);
            }
        }
    }


    private IHoldInteractable holdInteractable;
    private void TryInteractHold()
    {
        if (Physics.Raycast(look.transform.position, look.transform.forward, out RaycastHit hit, raycastDistance, LayerMask.GetMask("Interactable")))
        {
            if (hit.transform.TryGetComponent(out IHoldInteractable interactable))
            {
                holdProgress.fillAmount = interactable.holdProgress;
                holdInteractable = interactable;
                interactable.holdProgress += + holdSpeed * 120 * Time.deltaTime;
                interactable.OnHoldProgress(interactable.holdProgress);

                if(interactable.holdProgress >= 1)
                {
                    holdInteractable.OnHoldComplete(gameObject);
                    interactable.holdProgress = 0;
                }
                return;
            }
        }

        StopHoldInteract();
    }

    private void StopHoldInteract()
    {
        if(holdInteractable == null) 
            return;

        holdInteractable.holdProgress = 0;
        holdInteractable?.OnHoldProgress(0);
        holdInteractable = null;  

        holdProgress.fillAmount = 0;
    }


    private void TryInteractOnce()
    {
        if (Physics.Raycast(look.transform.position, look.transform.forward, out RaycastHit hit, raycastDistance, LayerMask.GetMask("Interactable", "Farmland")))
        {
            if (hit.transform.TryGetComponent(out IInteractableOnce interactable))
            {
                interactable.Interact(gameObject);
            }
        }
    }

    private void TryAltInteract()
    {
        if (Physics.Raycast(look.transform.position, look.transform.forward, out RaycastHit hit, raycastDistance, LayerMask.GetMask("Interactable", "Farmland")))
        {
            if (hit.transform.TryGetComponent(out IInteractableAlt interactable))
            {
                interactable.InteractAlt(gameObject);
            }
        }
    }

    private void HandleHighLight()
    {
        IHighlightable newHighlight = null;
        if (Physics.Raycast(look._mainCamera.transform.position, look._mainCamera.transform.forward, out RaycastHit hit, raycastDistance, highlightableLayers))
            hit.transform.TryGetComponent(out newHighlight);

        if (newHighlight != currentlyHighlightable)
        {
            currentlyHighlightable?.UnHighlight();
            currentlyHighlightable = newHighlight;
            currentlyHighlightable?.Highlight();

            if (currentlyHighlightable != null)
                crosshair.DOScale(new Vector3(2, 2, 2), 0.25f);
            else
                crosshair.DOScale(Vector3.one, 0.25f);
        }
    }
}
