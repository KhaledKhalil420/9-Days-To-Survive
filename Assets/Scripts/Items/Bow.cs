using EZCameraShake;
using Sortify;
using UnityEngine;

public class Bow : Item
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject arrow;
    private Transform _cam;
    private GameObject arrowPlaceHolder;
 
    [Header("Settings")]
    [SerializeField] private float holdTime;
    [SerializeField] private float arrowSpeed;
    [SerializeField] private float maxBonusSpeed;
    [SerializeField] private float maxBonusHoldTime;
    [SerializeField] private float arrowDamage;

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

    private void Start()
    {
        _cam = PlayerLook.mainCamera.transform;
    }

    public override void OnUse()
    {
        arrowPlaceHolder = Instantiate(arrow, transform.position, transform.rotation);
        arrowPlaceHolder.GetComponent<Arrow>().enabled = false;
        arrowPlaceHolder.transform.parent = transform;
        arrowPlaceHolder.layer = LayerMask.NameToLayer("Held");
        arrowPlaceHolder.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        arrowPlaceHolder.GetComponent<Collider>().isTrigger = true;
        arrowStartLocalPos = arrowPlaceHolder.transform.localPosition;
        
        source.PlayOneShot(drawSound);

        drawShake = CameraShaker.Instance.StartShake(drawShakeMagnitude, drawShakeRoughness, holdTime);
        drawShake.ScaleMagnitude = 0;
    }

    public override void OnUsing()
    {
        holdTimer += Time.deltaTime;
        animator.SetBool("Held", true);

        float chargeRatio = Mathf.Clamp01(holdTimer / (holdTime + maxBonusHoldTime));
        if (drawShake != null)
            drawShake.ScaleMagnitude = chargeRatio;

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

        animator.speed = 1;
        animator.SetBool("Held", false);
        holdTimer = 0;

        if (arrowPlaceHolder == null) return;
        Destroy(arrowPlaceHolder);
        arrowPlaceHolder = null;
    }

    void ShootArrow()
    {
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
        GameObject obj = Instantiate(arrow, _cam.position, _cam.rotation);
        obj.GetComponent<Rigidbody>().AddForce(_cam.forward * speed, ForceMode.Impulse);
        obj.GetComponent<Arrow>().damage = arrowDamage;
    }

    public override void OnChangingItems()
    {
        drawShake?.StartFadeOut(0.15f);
        drawShake = null;

        if (arrowPlaceHolder == null) 
            return;
            
        Destroy(arrowPlaceHolder);
        arrowPlaceHolder = null;
    }
}