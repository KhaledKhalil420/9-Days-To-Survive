using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private PlayerLook look;
    [SerializeField] private LayerMask highlightableLayers;
    [SerializeField] internal float raycastDistance;
    [SerializeField] private Transform crosshair;

    private float _eHoldTime = 0f;
    private const float HoldThreshold = 0.5f;

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
            _eHoldTime = 0f;
            TryInteract();
        }
        else if (Input.GetKey(Keybinds.Key("Interact")))
        {
            _eHoldTime += Time.deltaTime;
            if (_eHoldTime >= HoldThreshold)
            {
                TryInteract();
            }
        }
        else if (Input.GetKeyUp(Keybinds.Key("Interact")))
        {
            _eHoldTime = 0f;
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
