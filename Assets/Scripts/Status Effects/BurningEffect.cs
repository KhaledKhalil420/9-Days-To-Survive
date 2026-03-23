using UnityEngine;

public class BurningEffect : StatusEffect
{
    public IBurnable linkedBurnable;
    [SerializeField] private float fireDamage, fireIntreval;
    [SerializeField] private ParticleSystem fireParticles;
    private ParticleSystem spawnFireParticles;
    private float nextTick = 0;

    protected override void InitializeEffect(bool addedTo)
    {
        target.TryGetComponent(out linkedBurnable);

        if(linkedBurnable == null)
        {
            Expire();
        }

        nextTick = Time.time + fireIntreval;

        if(!addedTo)
        {
            spawnFireParticles = ParticleSpawner.SpawnWithBounds(fireParticles, target.transform.position, target.transform.rotation, target.GetComponent<Renderer>().bounds,
            transform, true, 2);
        }
    }

    protected override void UpdateEffect()
    {
        if(Time.time >= nextTick)
        {
            //Do Sound
            linkedBurnable.Burn(fireDamage);
            nextTick = Time.time + fireIntreval;
        }
    }

    protected override void OnExpire()
    {
        spawnFireParticles.Stop();
    }
}
