using UnityEngine;

public class PickableItem : MonoBehaviour, IInteractable
{
    [SerializeField] private Item item;

    public void Interact(GameObject sender)
    {
        if(sender.TryGetComponent(out PlayerInventory inventory))
        {
            Item _item = Instantiate(item);
            inventory.GiveItem(_item, out bool wasGiven);

            if(wasGiven)
            {
                AudioManager.Instance?.PlaySound("Pickup", 0.9f, 1.1f);
                Destroy(gameObject);
            }

            else
            {
                Destroy(_item);
            }
        }
    }
}
