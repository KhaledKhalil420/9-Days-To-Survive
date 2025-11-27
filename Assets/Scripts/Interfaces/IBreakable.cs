using UnityEngine;

public interface IBreakable
{
    public void Damage(GameObject sender, int damage, BreakableType type, int toughness);

}
