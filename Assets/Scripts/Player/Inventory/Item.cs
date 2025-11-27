using DG.Tweening;
using UnityEngine;

public class Item : MonoBehaviour, IHighlightable
{
    public GameObject heldby;
    public ItemData data;

    public bool isSingleQuantityItem = false;
    public int HeldQuantity;

    internal bool isItemPickedUp;


    public void UpdateHoldingItem(bool isHolding)
    {
        isItemPickedUp = isHolding;
        int targetLayer = LayerMask.NameToLayer(isHolding ? "Held" : "Pickable");

        GetComponent<Collider>().enabled = !isHolding;
        GetComponent<Rigidbody>().constraints = isHolding ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
        if(gameObject.TryGetComponent(out Animator animator)) animator.enabled = isHolding;
        gameObject.layer = targetLayer;

        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = targetLayer;
        }
    }

    public virtual void OnUse()
    {
    }

    public virtual void OnUsing()
    {
    }

    public virtual void OnUseAlt()
    {
    }

    public virtual void OnStoppingUse()
    {
    }

    public virtual void OnStoppingUseAlt()
    {
    }

    public virtual void OnUseMiddle()
    {
    }

    public virtual void OnSelect()
    {
    }

    public virtual void OnThrow()
    {
    }

    public virtual void OnChangingItems()
    {
    }

    public void SetItemParent(Transform parent)
    {
        transform.parent = parent;
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        UpdateHoldingItem(parent != null);
    }

    public void Highlight()
    {

    }

    public void UnHighlight()
    {

    }
    
    public virtual Item Clone()
    {
        return (Item)this.MemberwiseClone();
    }

}
