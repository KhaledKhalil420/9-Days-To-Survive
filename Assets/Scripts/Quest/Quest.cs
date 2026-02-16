using System;
using DG.Tweening;
using TMPro;
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
        float initPosition = transform.position.x;
        transform.localPosition -= new Vector3(500, 0);
        transform.DOMoveX(initPosition, 1).SetEase(Ease.InOutSine);

    }

    public void UpdateUi(string additionalText)
    {
        NameText.text = data.questName;
        descriptionText.text = data.description + " " + additionalText;
        imageIcon.sprite = data.sprite;
    }

    public void Close()
    {
        //Animation and close then spawn next quest
        DOVirtual.DelayedCall(2, () => transform.DOMoveX(-500, 1).SetEase(Ease.InOutSine).OnComplete(() => {Destroy(gameObject); questManager.CompleteQuest();}));
    }

    void OnDestroy()
    {
        DOTween.Kill(gameObject);
    }

    public void CompleteQuest()
    {
        isCompleted = true;
        CompletedText.text = "<Color=Green>Completed"; 
        Close();
    }
    
    public virtual void OnSpawned(){}
}