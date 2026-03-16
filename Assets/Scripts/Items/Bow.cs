using Sortify;
using UnityEngine;

public class Bow : Item
{
    #region References
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject arrow;
    private Transform _cam;
    private GameObject arrowPlaceHolder;
    #endregion

    #region Settings
    [Header("Settings")]
    [SerializeField] private float holdTime;
    [SerializeField] private float arrowSpeed;
    [SerializeField] private float maxBonusSpeed;
    [SerializeField] private float maxBonusHoldTime;
    [SerializeField] private float arrowDamage;

    [Header("Arrow Pull")]
    [SerializeField] private float nockOffset = 0.3f;
    [SerializeField] private float chargeOffset = 0.2f;
    #endregion

    #region State
    [SerializeField, ReadOnly] private float holdTimer;
    [SerializeField, ReadOnly] private bool isHeld;
    private Vector3 arrowStartLocalPos;
    #endregion

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
    }

    public override void OnUsing()
    {
        animator.speed = 1 / holdTime; 
        holdTimer += Time.deltaTime;
        animator.SetBool("Held", true);

        if (arrowPlaceHolder == null) return;

        if (holdTimer <= holdTime)
        {
            float nockRatio = holdTimer / holdTime;
            arrowPlaceHolder.transform.localPosition = arrowStartLocalPos - new Vector3(0, 0, nockRatio * nockOffset);
        }
        else
        {
            float chargeRatio = Mathf.Clamp01((holdTimer - holdTime) / maxBonusHoldTime);
            arrowPlaceHolder.transform.localPosition = arrowStartLocalPos - new Vector3(0, 0, nockOffset + chargeRatio * chargeOffset);
        }
    }

    public override void OnStoppingUse()
    {
        if (holdTimer >= holdTime)
            ShootArrow();

        animator.speed = 1;
        animator.SetBool("Held", false);
        holdTimer = 0;

        if (arrowPlaceHolder == null) return;
        Destroy(arrowPlaceHolder);
        arrowPlaceHolder = null;
    }

    void ShootArrow()
    {
        animator.SetTrigger("Shoot");

        float chargeRatio = Mathf.Clamp01((holdTimer - holdTime) / maxBonusHoldTime);
        float speed = arrowSpeed + maxBonusSpeed * chargeRatio;

        Vector3 spawnPoint = new Vector3(_cam.position.x, _cam.position.y, _cam.position.z);
        GameObject obj = Instantiate(arrow, spawnPoint, _cam.rotation);
        obj.GetComponent<Rigidbody>().AddForce(_cam.forward * speed, ForceMode.Impulse);
        obj.GetComponent<Arrow>().damage = arrowDamage;
    }
}