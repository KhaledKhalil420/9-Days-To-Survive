using DG.Tweening;
using EZCameraShake;
using UnityEngine;

public class BreakingTool : Item
{
    [SerializeField] private BreakableType type;
    [SerializeField] private Animator animator;
    [SerializeField] private float range = 2f, cooldown = 1f;
    [SerializeField] private int damage = 50;
    public int toughness = 1;
    private Transform cam;
    private bool canUse = true;

    void Start() 
    {
        cam = PlayerLook.mainCamera.transform;
    }

    public override void OnUsing()
    {
        if(!canUse)
            return;

        //Play animation
        animator.SetTrigger("Trigger");
        canUse = false;
        DOVirtual.DelayedCall(cooldown, () => canUse = true);
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
}