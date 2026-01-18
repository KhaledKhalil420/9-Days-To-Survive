using UnityEngine;

public class Consumables : Item
{
    [SerializeField] private float timetoConsume = 1;
    [SerializeField] protected Animator anim;

    protected PlayerStats playerStats;

    public override void OnUse()
    {
        anim.SetBool("Trigger", true);
        Invoke(nameof(Consume), timetoConsume);
    }

    public override void OnStoppingUse()
    {
        anim.SetBool("Trigger", false);
        CancelInvoke(nameof(Consume));
    }

    public void Consume()
    {
        if(playerStats == null)
        {
            if(heldby.TryGetComponent(out PlayerStats stats))
            {
                playerStats = stats;
            }
        }

        anim.SetBool("Trigger", false);
        parentSlot.HeldQuantity -= 1; 

        OnConsume();
    }

    public virtual void OnConsume()
    {
        
    }
}
