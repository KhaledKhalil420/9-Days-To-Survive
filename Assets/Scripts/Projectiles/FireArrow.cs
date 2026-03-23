using UnityEngine;

public class FireArrow : Arrow
{
    [SerializeField] private StatusEffect fireStatusEffect;

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

        if(collider.TryGetComponent(out IBurnable burnable))
        {
            if(collider.TryGetComponent(out StatusEffectTarget target))
            {
                target.AddStatusEffect(fireStatusEffect);
            }
            else
            {
                
                burnable.Burn();
            }
        }
        

        Stick(collider.transform);
    }
}
