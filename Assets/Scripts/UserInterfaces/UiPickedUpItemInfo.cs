using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiPickedUpItemInfo : MonoBehaviour
{
    public Image icon;
    public TMP_Text text;
    [SerializeField] float disappearAfter = 2f;
    public CanvasGroup group;

    Tween fadeTween;
    int quantity;
    string itemKey;
    public System.Action onFinished;

    public void Init(ItemData item, int startQuantity)
    {
        itemKey = item.Name;
        quantity = startQuantity;

        icon.sprite = item.sprite;
        UpdateText();

        if (group != null)
            group.alpha = 1f;
        else
        {
            icon.canvasRenderer.SetAlpha(1f);
            text.canvasRenderer.SetAlpha(1f);
        }

        StartDisappearTween();
    }

    public void AddQuantity(int add)
    {
        quantity += add;
        UpdateText();

        if (group != null)
            group.alpha = 1f;
        else
        {
            icon.canvasRenderer.SetAlpha(1f);
            text.canvasRenderer.SetAlpha(1f);
        }

        StartDisappearTween();
    }

    void UpdateText()
    {
        text.text = itemKey + " x" + quantity.ToString();
    }

    void StartDisappearTween()
    {
        fadeTween?.Kill();

        if (group != null)
        {
            fadeTween = group.DOFade(0f, disappearAfter).SetEase(Ease.Linear).OnComplete(() =>
            {
                onFinished?.Invoke();
                Destroy(gameObject);
            });
            return;
        }

        icon.DOKill();
        text.DOKill();

        var seq = DOTween.Sequence();
        seq.Append(icon.DOFade(0f, disappearAfter));
        seq.Join(text.DOFade(0f, disappearAfter));
        seq.OnComplete(() =>
        {
            onFinished?.Invoke();
            Destroy(gameObject);
        });

        fadeTween = seq;
    }

    void OnDestroy()
    {
        fadeTween?.Kill();
        if (group != null) group.DOKill();
        icon.DOKill();
        text.DOKill();
    }
}
