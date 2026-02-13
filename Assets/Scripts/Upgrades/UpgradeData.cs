using UnityEngine;

[CreateAssetMenu]
public class UpgradeData : ScriptableObject
{
    public string fullName;
    public int price;
    [TextArea] public string discription;
    public Sprite sprite; 
    public GameObject upgrade;
}
