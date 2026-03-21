using System.Collections;
using DG.Tweening;
using EZCameraShake;
using Sortify;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.Rendering;

public class Bow : Item
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Item arrow;
    [SerializeField] private Volume volume;
    private Transform _cam;
    private PlayerInventory inventory;
    private GameObject arrowPlaceHolder;

    [Header("Settings")]
    [SerializeField] private float holdTime;
    [SerializeField] private float arrowSpeed;
    [SerializeField] private float maxBonusSpeed;
    [SerializeField] private float maxBonusHoldTime;
    [SerializeField] private float arrowDamage;
    private bool canUse = false;

    [Header("Arrow Pull")]
    [SerializeField] private float nockOffset = 0.3f;
    [SerializeField] private float chargeOffset = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip drawSound;
    [SerializeField] private AudioClip shootSound;

    [Header("Camera Shake")]
    [SerializeField] private float drawShakeMagnitude = 0.3f;
    [SerializeField] private float drawShakeRoughness = 2f;
    [SerializeField] private float shootShakeMagnitude = 2f;
    [SerializeField] private float shootShakeRoughness = 4f;
    [SerializeField] private float shootShakeFadeOut = 0.5f;

    [SerializeField, ReadOnly] private float holdTimer;
    [SerializeField, ReadOnly] private bool isHeld;
    private Vector3 arrowStartLocalPos;
    private CameraShakeInstance drawShake;
    private Tweener volumeTween;

    public override void OnPick()
    {
        heldby.TryGetComponent(out inventory);
    }

    private void Start()
    {
        _cam = PlayerLook.mainCamera.transform;
    }

    public override void OnUse()
    {        
        if(!inventory.HasItem(arrow, 1)) 
            return;
        
        canUse = true;
        arrowPlaceHolder = Instantiate(arrow.gameObject, transform);
        arrowPlaceHolder.layer = LayerMask.NameToLayer("Held");
        arrowPlaceHolder.GetComponent<Arrow>().asItem = false;
        arrowPlaceHolder.GetComponent<Arrow>().enabled = false;
        arrowPlaceHolder.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        arrowPlaceHolder.GetComponent<Collider>().isTrigger = true;
        arrowPlaceHolder.transform.localPosition = Vector3.zero;
        arrowPlaceHolder.transform.localRotation = Quaternion.identity;

        source.PlayOneShot(drawSound);

        drawShake = CameraShaker.Instance.StartShake(drawShakeMagnitude, drawShakeRoughness, holdTime);
        drawShake.ScaleMagnitude = 0;
    }

    public override void OnUsing()
    {
        if(!canUse) 
            return;

        holdTimer += Time.deltaTime;
        animator.SetBool("Held", true);
        arrowPlaceHolder.transform.localRotation = Quaternion.identity;

        float chargeRatio = Mathf.Clamp01(holdTimer / (holdTime + maxBonusHoldTime));

        if (drawShake != null)
            drawShake.ScaleMagnitude = chargeRatio;

        if (volume != null)
        {
            volumeTween?.Kill();
            volume.weight = chargeRatio;
        }

        if (arrowPlaceHolder == null) return;

        if (holdTimer <= holdTime)
        {
            float nockRatio = holdTimer / holdTime;
            arrowPlaceHolder.transform.localPosition = arrowStartLocalPos - new Vector3(0, 0, nockRatio * nockOffset);
        }
        else
        {
            float overchargeRatio = Mathf.Clamp01((holdTimer - holdTime) / maxBonusHoldTime);
            arrowPlaceHolder.transform.localPosition = arrowStartLocalPos - new Vector3(0, 0, nockOffset + overchargeRatio * chargeOffset);
        }
    }

    public override void OnStoppingUse()
    {
        if (holdTimer >= holdTime)
            ShootArrow();

        drawShake?.StartFadeOut(0.15f);
        drawShake = null;

        FadeOutVolume(0.15f);

        animator.speed = 1;
        animator.SetBool("Held", false);
        holdTimer = 0;

        if (arrowPlaceHolder == null) return;
        Destroy(arrowPlaceHolder);
        arrowPlaceHolder = null;
    }

    private void ShootArrow()
    {
        canUse = false;
        inventory.TakeItem(arrow, 1, out bool wasTaken);


        source.PlayOneShot(shootSound);

        float chargeRatio = Mathf.Clamp01((holdTimer - holdTime) / maxBonusHoldTime);

        CameraShaker.Instance.ShakeOnce(
            shootShakeMagnitude * (1 + chargeRatio),
            shootShakeRoughness,
            0f,
            shootShakeFadeOut
        );

        animator.SetTrigger("Shoot");

        float speed = arrowSpeed + maxBonusSpeed * chargeRatio;
        GameObject obj = Instantiate(arrow.gameObject, _cam.position, _cam.rotation);
        obj.GetComponent<Arrow>().asItem = false;
        obj.GetComponent<Rigidbody>().AddForce(_cam.forward * speed, ForceMode.Impulse);
        obj.GetComponent<Arrow>().damage = arrowDamage + ((arrowDamage * inventory.damageBonus) / 2);;
    }

    public override void OnChangingItems()
    {
        drawShake?.StartFadeOut(0.15f);
        drawShake = null;

        FadeOutVolume(0.15f);

        animator.speed = 1;
        animator.SetBool("Held", false);
        holdTimer = 0;

        if (arrowPlaceHolder == null)
            return;

        Destroy(arrowPlaceHolder);
        arrowPlaceHolder = null;
    }

    private void FadeOutVolume(float duration)
    {
        if (volume == null) return;
        volumeTween?.Kill();
        volumeTween = DOTween.To(() => volume.weight, x => volume.weight = x, 0f, duration).SetEase(Ease.OutQuad);
    }
}