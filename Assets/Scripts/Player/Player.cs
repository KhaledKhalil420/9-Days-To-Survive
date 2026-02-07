using UnityEngine;

public class Player : MonoBehaviour
{
    public static PlayerMovement movement;
    public static PlayerInteract interact;
    public static PlayerInventory inventory;
    public static PlayerStats stats;
    public static PlayerLook look;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        interact = GetComponent<PlayerInteract>();
        inventory = GetComponent<PlayerInventory>();
        stats = GetComponent<PlayerStats>();
        look = GetComponent<PlayerLook>();
    }
}