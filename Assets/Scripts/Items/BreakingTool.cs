using DG.Tweening;
using EZCameraShake;
using JetBrains.Annotations;
using UnityEngine;

public class BreakingTool : Item
{
    [SerializeField] private AudioSource source;
    [SerializeField] private BreakableType type;
    [SerializeField] private Animator animator;
    [SerializeField] private int numberOfAnimations;
    [SerializeField] private float range = 2f, cooldown = 1f;
    [SerializeField] private int damage = 50;
    public int toughness = 1;
    private Transform cam;
    private bool canUse = true;
    private int lastAnimationIndex = -1;

    void Start() 
    {
        cam = PlayerLook.mainCamera.transform;
    }

    public override void OnUsing()
    {
        if(!canUse)
            return;

        canUse = false;

        int randomAnimation = GetRandomAnimationIndex();
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

        if (hit.transform.TryGetComponent<IBreakable>(out var damagable))
        {
            damagable.Damage(heldby, damage, type, toughness);
        }
    }

    public void Play()
    {
        source.Play();
    }
}