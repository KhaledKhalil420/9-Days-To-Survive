using UnityEngine;

public class Food : Consumables
{
    [SerializeField] private float health, satiation;
    private PlayerStats playerStats;

    public override void OnConsume()
    {
        playerStats.Heal(health);
        playerStats.Eat(satiation);
        playerStats.stamina.Modify(satiation);
    }
}
