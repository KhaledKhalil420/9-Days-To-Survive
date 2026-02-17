using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        StartCoroutine(nameof(LateStart));
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
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
        
        if(turnOn && questIndex < quests.Count) 
            SpawnQuest(quests[questIndex]);
    }
}