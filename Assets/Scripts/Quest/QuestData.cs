using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest System/Quest Data")]
public class QuestData : ScriptableObject
{
    public string questName;
    [TextArea(2, 4)]
    public string description;
    public Sprite sprite;
}