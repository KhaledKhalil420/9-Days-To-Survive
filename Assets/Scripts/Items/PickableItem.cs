using UnityEngine;

public class PickableItem : MonoBehaviour, IInteractable
{
    [SerializeField] private Item item;
    [SerializeField] private GameObject objectToDestroy; //Leave null for full destruction

    public void Interact(GameObject sender)
    {
        if(sender.TryGetComponent(out PlayerInventory inventory))
        {
            Item _item = Instantiate(item);
            inventory.GiveItem(_item, out bool wasGiven);

            if(wasGiven)
            {
                AudioManager.Instance?.PlaySound("Pickup", 0.9f, 1.1f);

                if(objectToDestroy == null)
                Destroy(gameObject);

                else
                Destroy(objectToDestroy);

                Destroy(this);
            }

            else
            {
                Destroy(_item);
            }
        }
    }
}
