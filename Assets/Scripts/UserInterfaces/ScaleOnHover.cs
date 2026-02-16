using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScaleOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 initSize;
    [SerializeField] private float strength = 1.05f;
    [SerializeField] private bool playSound = false;

    private void Start()
    {
        initSize = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(initSize * strength, 0.5f);

        if(playSound) AudioManager.Instance.PlaySound("Ui_Hover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(initSize, 0.5f);
    }

    void OnDestroy()
    {
        DOTween.Kill(gameObject);
    }
}
