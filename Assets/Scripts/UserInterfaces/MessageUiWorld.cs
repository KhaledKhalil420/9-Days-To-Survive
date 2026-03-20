using DG.Tweening;
using TMPro;
using UnityEngine;

public class MessageUiWorld : MonoBehaviour
{
    public string message = "";
    public Transform camTransform;

    void Start()
    {
        camTransform = PlayerLook.mainCamera.transform;

        GetComponentInChildren<TMP_Text>().text = message;
        transform.DOMoveY(transform.position.y + 0.15f, 2);
        GetComponent<CanvasGroup>().DOFade(0, 1.5f);
        DOVirtual.DelayedCall(2, () => Destroy(gameObject)).OnComplete(() => DOTween.Kill(this));   

    }

    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - camTransform.position);
    }
}
