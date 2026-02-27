using DG.Tweening;
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
        look = FindAnyObjectByType<PlayerLook>();
    }

    private void OnValidate()
    {
        movement = GetComponent<PlayerMovement>();
        interact = GetComponent<PlayerInteract>();
        inventory = GetComponent<PlayerInventory>();
        stats = GetComponent<PlayerStats>();
        look = FindAnyObjectByType<PlayerLook>();
    }

    public void Disable()
    {
        movement.enabled = false;
        interact.enabled = false;
        inventory.enabled = false;
        stats.enabled = false;
        look.enabled = false;
    }

    void OnDestroy()
    {
        DOTween.Kill(gameObject);
    }
}