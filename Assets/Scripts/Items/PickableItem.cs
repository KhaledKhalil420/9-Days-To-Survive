using UnityEngine;

public class PickableItem : MonoBehaviour, IHoldInteractable
{
    [SerializeField] private Item item;
    [SerializeField] private GameObject objectToDestroy;

    public float HoldDuration => 0.5f;

    public float holdProgress { get; set;}

    public void OnHoldComplete(GameObject sender)
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

    public void OnHoldProgress(float progress)
    {
        //Sound or something
    }
}
