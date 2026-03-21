using UnityEngine;

public class Arrow : Item
{
    internal Rigidbody rb;

    [Header("Arrow Settings")]
    [SerializeField] internal bool asItem = true;
    [SerializeField] private float pierceDepth = 0.2f;
    [SerializeField] private float disappearDelay = 10f;
    [SerializeField] private float alignSpeed = 13;

    internal float damage;
    private bool hasHit = false;

    private void Start()
    {
        if(asItem)
            return;
        
        gameObject.layer = 0;
        rb = GetComponent<Rigidbody>();
        GetComponent<AudioSource>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<TrailRenderer>().enabled = false;
        Collider col = GetComponent<Collider>();
        col.enabled = false;
        col.isTrigger = true;
        GetComponent<MeshRenderer>().enabled = false;
        Invoke(nameof(EnableCollider), 0.015f);
    }

    private void FixedUpdate()
    {
        if (asItem || hasHit) return;

        if (rb.linearVelocity.sqrMagnitude > 0.01f)
    {
        Quaternion targetRot = Quaternion.LookRotation(rb.linearVelocity.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * alignSpeed);
    }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(asItem) return;
        if (collider.CompareTag("Player") || hasHit) return;

        hasHit = true;
        
        if(collider.TryGetComponent(out Damagable damagable))
        {
            if(damagable.isEnemy)
            damagable.Damage(damage);
        }
        

        Stick(collider.transform);
    }

    private void Stick(Transform hitTransform) 
    {
        transform.position += transform.forward * pierceDepth;

        Destroy(rb);
        Destroy(GetComponent<Collider>());
        Destroy(GetComponent<TrailRenderer>());

        transform.SetParent(hitTransform, true);

        Invoke(nameof(Disappear), disappearDelay);
    }

    private void EnableCollider()
    {
        GetComponent<AudioSource>().enabled = true;
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<TrailRenderer>().enabled = true;
        Collider col = GetComponent<Collider>();
        col.enabled = true;
    }

    private void Disappear() => Destroy(gameObject);

    private void OnDisable()
    {
        if(asItem) return;
        if (!hasHit) return;
        CancelInvoke();
        Destroy(gameObject);
    }
}