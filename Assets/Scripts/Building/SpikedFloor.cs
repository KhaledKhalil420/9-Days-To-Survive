using UnityEngine;
using DG.Tweening;

public class SpikedFloor : Building
{
    [SerializeField] private int damage;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private SkinnedMeshRenderer skinnedMesh;
    [SerializeField] private AudioSource spikesSource;
    
    private Collider[] hitBuffer = new Collider[10];
    private float nextDamageTime;
    private Vector3 halfExtents;
    private bool isReady = true;
    private Tween currentTween;

    void Start()
    {
        halfExtents = skinnedMesh.bounds.extents;
        skinnedMesh.SetBlendShapeWeight(0, 0f);
    }

    void Update()
    {
        if (!isReady)
        {
            if (Time.time >= nextDamageTime)
            {
                isReady = true;
            }
            return;
        }
        
        int hitCount = Physics.OverlapBoxNonAlloc(transform.position, halfExtents, hitBuffer, Quaternion.identity, LayerMask.GetMask("Enemy"));
        
        if (hitCount > 0) 
        { 
            for (int i = 0; i < hitCount; i++)
                if (hitBuffer[i].TryGetComponent(out IDamagable damagable)) damagable.Damage(damage + extraDamage);
            
            Damage(1);
            isReady = false;
            nextDamageTime = Time.time + damageInterval;
            
            currentTween?.Kill();
            skinnedMesh.SetBlendShapeWeight(0, -100f);

            currentTween?.Kill();
            currentTween = DOTween.To(() => skinnedMesh.GetBlendShapeWeight(0), x => skinnedMesh.SetBlendShapeWeight(0, x), 0f, damageInterval * 1.5f).SetEase(Ease.OutQuad).SetRecyclable(true);
            
            spikesSource.pitch = Random.Range(0.95f, 1.05f);
            spikesSource.Play();
        }
    }

    public override void OnDeath() => currentTween?.Kill();
}