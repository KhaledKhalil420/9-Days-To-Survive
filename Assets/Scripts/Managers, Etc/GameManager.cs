using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject player;

    public List<Item> starterItems;

    private void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
    }

    void Start()
    {
        foreach(Item item in starterItems)
        player.GetComponent<PlayerInventory>().GiveItem(item);
    }
}
