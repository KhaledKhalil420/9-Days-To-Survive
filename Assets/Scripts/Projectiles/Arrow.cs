using UnityEngine;

public class Arrow : MonoBehaviour
{
    internal Rigidbody rb;

    [Header("Arrow Settings")]
    [SerializeField] private float pierceDepth = 0.2f;
    [SerializeField] private float disappearDelay = 10f;

    internal float damage;
    private bool hasHit;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        GetComponent<MeshRenderer>().enabled = false;
        Invoke(nameof(EnableCollider), 0.015f);
    }

    private void Update()
    {
        if (hasHit) return;
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
            transform.forward = rb.linearVelocity.normalized;
    }

    void EnableCollider()
    {
        GetComponent<AudioSource>().enabled = true;
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<TrailRenderer>().enabled = true;
        Collider col = GetComponent<Collider>();
        col.enabled = true;
        col.isTrigger = false;
    }

    void OnCollisionEnter(Collision collision)
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

    void Stick(Transform hitTransform)
    {
        transform.position += transform.forward * pierceDepth;
        transform.SetParent(hitTransform);

        rb.linearVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        GetComponent<Collider>().isTrigger = true;

        Invoke(nameof(Disappear), disappearDelay);
    }

    void Disappear() => Destroy(gameObject);

    private void OnDisable()
    {
        if (!hasHit) return;
        CancelInvoke();
        Destroy(gameObject);
    }
}