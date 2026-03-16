using UnityEngine;

public class Arrow : MonoBehaviour
{
    #region References
    internal Rigidbody rb;
    #endregion

    #region Settings
    [Header("Arrow Settings")]
    [SerializeField] private float pierceDepth = 0.2f;
    [SerializeField] private float disappearDelay = 10f;
    #endregion

    #region State
    internal float damage;
    private bool hasHit;
    #endregion

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Invoke(nameof(EnableCollider), 0.05f);
    }

    private void Update()
    {
        if (hasHit) return;
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
            transform.forward = rb.linearVelocity.normalized;
    }

    void EnableCollider()
    {
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
            collision.collider.GetComponent<IDamagable>()?.Damage(damage);
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
}