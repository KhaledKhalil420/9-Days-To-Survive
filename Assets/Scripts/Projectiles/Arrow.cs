using UnityEngine;

public class Arrow : Item
{
    internal Rigidbody rb;

    [Header("Arrow Settings")]
    [SerializeField] internal bool asItem = true;
    [SerializeField] private float pierceDepth = 0.2f;
    [SerializeField] private float disappearDelay = 10f;

    internal float damage;
    private bool hasHit = false;

    private void Start()
    {
        if(asItem) return;
        rb = GetComponent<Rigidbody>();
        GetComponent<MeshRenderer>().enabled = false;
        Invoke(nameof(EnableCollider), 0.015f);
    }

    private void Update()
    {
        if(asItem) return;
        if (hasHit)
        {
            return;
        }
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
            transform.forward = rb.linearVelocity.normalized;
    }

    private void EnableCollider()
    {
        GetComponent<AudioSource>().enabled = true;
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<TrailRenderer>().enabled = true;
        Collider col = GetComponent<Collider>();
        col.enabled = true;
        col.isTrigger = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player") || hasHit) return;

        hasHit = true;

        if (rb.linearVelocity.magnitude > 0.5f)
        {
            if(collision.collider.TryGetComponent(out Damagable damagable))
            {
                if(damagable.isEnemy)
                damagable.Damage(damage);
            }
        }

        Stick(collision.transform);
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

    private void Disappear() => Destroy(gameObject);

    private void OnDisable()
    {
        if (!hasHit) return;
        CancelInvoke();
        Destroy(gameObject);
    }
}