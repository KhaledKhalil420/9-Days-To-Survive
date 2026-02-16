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
        questManager = _questManager;
        OnSpawned();
    }

    public void UpdateUi(string additionalText)
    {
        NameText.text = data.questName;
        descriptionText.text = data.description + " " + additionalText;
        imageIcon.sprite = data.sprite;
    }

    public void Close()
    {
        transform.DOMoveX(-500, 1).OnComplete(() => {Destroy(gameObject); questManager.CompleteQuest();});
    }

    public void CompleteQuest()
    {
        isCompleted = true;
        CompletedText.text = "<Color=Green>Completed<Color>"; 
        Close();
    }
    
    public virtual void OnSpawned(){}
}