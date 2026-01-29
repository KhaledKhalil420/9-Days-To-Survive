using UnityEngine;

public class SpikedFloor : Building
{
    [SerializeField] private int damage;
    [SerializeField] private float damageInterval = 0.5f;
    
    private Collider[] hitBuffer = new Collider[10];
    private float nextDamageTime;
    private Vector3 halfExtents;

    void Start()
    {
        halfExtents = transform.localScale * 0.1f;
    }

    void Update()
    {
        if (Time.time < nextDamageTime) return;
        
        int hitCount = Physics.OverlapBoxNonAlloc(transform.position, halfExtents, hitBuffer, Quaternion.identity, LayerMask.GetMask("Enemy"));
        
        for (int i = 0; i < hitCount; i++)
            if (hitBuffer[i].TryGetComponent(out IDamagable damagable)) damagable.Damage(damage + extraDamage);
        
        if (hitCount > 0) { currentHealth -= 1; nextDamageTime = Time.time + damageInterval; }
    }
}