using UnityEngine;

public class BurningEffect : StatusEffect
{
    public IBurnable linkedBurnable;
    [SerializeField] private float fireDamage, fireIntreval;
    [SerializeField] private ParticleSystem fireParticles;
    private float nextTick = 0;

    protected override void InitializeEffect()
    {
        target.TryGetComponent(out linkedBurnable);

        if(linkedBurnable == null)
        {
            Expire();
        }

        nextTick = Time.time + fireIntreval;
    }

    protected override void UpdateEffect()
    {
        if(Time.time >= nextTick)
        {
            //Do particle effect
            //Do Sound
            linkedBurnable.Burn(fireDamage);
            nextTick = Time.time + fireIntreval;
        }
        
    }
}
