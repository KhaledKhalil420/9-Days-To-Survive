using UnityEngine;

public class FireArrow : Arrow
{
    //Overrinng the on trigger stay here..
    private void OnTriggerStay(Collider collider)
    {
        if(asItem) return;
        if (collider.CompareTag("Player") || hasHit) return;

        rb.linearVelocity = Vector3.zero;
        hasHit = true;
        
        if(collider.TryGetComponent(out Damagable damagable))
        {
            if(damagable.isEnemy)
            damagable.Damage(damage);
        }

        if(collider.TryGetComponent(out HardwoodTree hardwoodTree))
        {
            hardwoodTree.BreakBarier();
        }
        

        Stick(collider.transform);
    }
}
