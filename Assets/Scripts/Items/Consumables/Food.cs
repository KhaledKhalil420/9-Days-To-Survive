using UnityEngine;

public class Food : Consumables
{
    [SerializeField] private float health, satiation;
    private PlayerStats playerStats;

    public override void OnConsume()
    {
        if(playerStats == null)
        {
            if(heldby.TryGetComponent(out PlayerStats stats))
            {
                playerStats = stats;
            }
        }

        playerStats.Heal(health);
        playerStats.Eat(satiation);
        playerStats.stamina.Modify(satiation);
    }
}
