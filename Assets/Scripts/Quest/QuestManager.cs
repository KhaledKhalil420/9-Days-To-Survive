using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    [SerializeField] private List<Quest> quests;
    [SerializeField] private int questIndex = 0;
    [SerializeField] private Transform parent;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnQuest(quests[questIndex]);
    }
    
    private void SpawnQuest(Quest quest)
    {
        Quest spawnedQuest = Instantiate(quest, parent).GetComponent<Quest>();
        spawnedQuest.Bootstrap(this);
    }

    public void CompleteQuest()
    {
        //Spawn next quest
        if(questIndex < quests.Count) 
            SpawnQuest(quests[questIndex]);
    }
}