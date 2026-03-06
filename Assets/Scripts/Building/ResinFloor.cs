using UnityEngine;

public class ResinFloor : MonoBehaviour
{
    [SerializeField] private float slowMultiplier = 0.4f;
    [SerializeField] private SkinnedMeshRenderer meshRenderer;

    private Collider[] hitBuffer = new Collider[10];
    private Vector3 halfExtents;

    void Start()
    {
        halfExtents = meshRenderer.bounds.extents;
    }

    void Update()
    {
        int hitCount = Physics.OverlapBoxNonAlloc(transform.position, halfExtents, hitBuffer, Quaternion.identity, LayerMask.GetMask("Enemy"));

        for (int i = 0; i < hitBuffer.Length; i++)
        {
            if (hitBuffer[i] == null) continue;

            if (i < hitCount)
            {
                if (hitBuffer[i].TryGetComponent(out GroundEnemy enemy))
                    enemy.speedModifier = slowMultiplier;
            }
            else
            {
                if (hitBuffer[i].TryGetComponent(out GroundEnemy enemy))
                    enemy.speedModifier = 1f;

                hitBuffer[i] = null;
            }
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < hitBuffer.Length; i++)
        {
            if (hitBuffer[i] != null && hitBuffer[i].TryGetComponent(out GroundEnemy enemy))
                enemy.speedModifier = 1f;

            hitBuffer[i] = null;
        }
    }
}