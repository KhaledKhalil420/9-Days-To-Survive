using UnityEngine;

public interface IBreakable
{
    public void Damage(GameObject sender, float damage, BreakableType type, int toughness);

}
