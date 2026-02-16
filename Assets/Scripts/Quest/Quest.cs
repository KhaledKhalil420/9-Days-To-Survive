using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Quest : MonoBehaviour
{
    internal QuestManager questManager;
    internal bool isCompleted = false;
    [SerializeField] internal QuestData data;
    [SerializeField] internal Image imageIcon;
    [SerializeField] internal TMP_Text NameText, descriptionText, CompletedText;
    
    public void Bootstrap(QuestManager _questManager)
    {
        UpdateUi("");
        questManager = _questManager;
        OnSpawned();

        //Animation
        Vector3 initPosition = transform.position;
        transform.localPosition -= new Vector3(500, 0);
        transform.DOMove(initPosition, 1).SetUpdate(true);

    }

    public void UpdateUi(string additionalText)
    {
        NameText.text = data.questName;
        descriptionText.text = data.description + " " + additionalText;
        imageIcon.sprite = data.sprite;
    }

    public void Close()
    {
        DOVirtual.DelayedCall(1f, () =>
        {
            transform.DOMoveX(-500, 1f)
            .OnComplete(() =>
            {
                questManager.CompleteQuest();
                Destroy(gameObject);
            });
        })
        .SetTarget(this).SetUpdate(true);
    }

    void OnDestroy()
    {
        DOTween.Kill(this);
    }

    public void CompleteQuest()
    {
        isCompleted = true;
        CompletedText.text = "<Color=Green>Completed"; 
        Close();
    }
    
    public virtual void OnSpawned(){}
}