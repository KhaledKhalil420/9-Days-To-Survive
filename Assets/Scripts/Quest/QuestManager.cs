using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    public bool turnOn = true;
    [SerializeField] private List<Quest> quests = new();
    [SerializeField] private int questIndex = 0;
    [SerializeField] private Transform parent;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if(turnOn)
        SpawnQuest(quests[0]);
    }
    
    private void SpawnQuest(Quest quest)
    {
        Quest spawnedQuest = Instantiate(quest, parent).GetComponent<Quest>();
        spawnedQuest.Bootstrap(this);
    }

    public void CompleteQuest()
    {
        //Spawn next quest
        questIndex++;
        
        if(questIndex < quests.Count) 
            SpawnQuest(quests[questIndex]);
    }
}