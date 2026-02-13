using UnityEngine;

public class BreakingTool : Item
{
    [SerializeField] private AudioSource source;
    [SerializeField, EditorChangeable] private BreakableType type;
    [SerializeField] private Animator animator;
    [SerializeField] private int numberOfAnimations;
    [SerializeField, EditorChangeable] private float range = 2f, cooldown = 1f;
    [SerializeField, EditorChangeable] private int damage = 50, minimumDamage = 1, maximumDamage = 1;
    [SerializeField] private Texture sphereIcon;
    [EditorChangeable] public int toughness = 1;
    private Transform cam;
    private bool canUse = true;
    private int lastAnimationIndex = -1;

    [SerializeField] private ParticleSystem hitParticlesPrefab;

    void Start()
    {
        cam = PlayerLook.mainCamera.transform;
    }

    public override void OnUsing()
    {
        if (!canUse)
            return;
        if(cam == null)
            cam = PlayerLook.mainCamera.transform;

        canUse = false;

        int randomAnimation = GetRandomAnimationIndex();
        animator.speed = 1 / (cooldown / heldby.GetComponent<PlayerInventory>().speedBonus);
        animator.SetInteger("Numb", randomAnimation);
        animator.SetTrigger("Trigger");

        Invoke(nameof(ResetCoolDown), cooldown);
    }

    public void ResetCoolDown()
    {
        canUse = true;
    }

    public override void OnStoppingUse()
    {
        animator.ResetTrigger("Trigger");
    }

    private int GetRandomAnimationIndex()
    {
        if (numberOfAnimations <= 1)
            return 1;

        int randomIndex;
        do
        {
            randomIndex = Random.Range(1, numberOfAnimations + 1);
        }
        while (randomIndex == lastAnimationIndex);

        lastAnimationIndex = randomIndex;
        return randomIndex;
    }

    public void Use()
    {
        if (!Physics.Raycast(cam.position, cam.forward, out var hit, range))
            return;

        Renderer sourceRenderer = hit.transform.GetComponentInChildren<Renderer>();
        Material sourceMat = sourceRenderer != null && sourceRenderer.sharedMaterial != null ? sourceRenderer.sharedMaterial : null;
        float calculatedDamage = damage * Random.Range(minimumDamage, maximumDamage) * heldby.GetComponent<PlayerInventory>().damageBonus;

        if (hit.transform.TryGetComponent<IBreakable>(out var breakable))
        {
            breakable.Damage(heldby, calculatedDamage, type, toughness);

            if (hitParticlesPrefab != null)
                SpawnHitParticles(hit.point, hit.normal, sourceMat);
        }

        if (hit.transform.TryGetComponent<IDamagable>(out var damagable))
        {
            damagable.Damage((float)damage / 3); //small debuf for enemies
        }
    }

    void SpawnHitParticles(Vector3 position, Vector3 normal, Material mat)
    {
        var go = Instantiate(hitParticlesPrefab.gameObject, position, Quaternion.LookRotation(normal));
        var pr = go.GetComponent<ParticleSystemRenderer>();
        var ps = go.GetComponent<ParticleSystem>();

        pr.material = mat;
        pr.material.mainTexture = sphereIcon;


        if (ps != null)
        {
            var main = ps.main;
            ps.Play();
            float life = main.duration + (main.startLifetime.mode == ParticleSystemCurveMode.Constant ? main.startLifetime.constant : main.startLifetime.constantMax);
            Destroy(go, life + 0.5f);
        }
        else
        {
            Destroy(go, 5f);
        }
    }

    public void Play()
    {
        source.Play();
    }
}
