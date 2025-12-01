using UnityEngine;

public enum BreakableType {Pickaxe, Axe, Else, Buildings}
public class Breakable : MonoBehaviour, IBreakable
{
    [SerializeField] private AudioSource source;
    [SerializeField] private BreakableType requiredTool;
    [SerializeField] private int toughness, health;
    [SerializeField] protected Item item;
    private GameObject sender;

    public void Damage(GameObject sender, int damage, BreakableType _type, int _toughness)
    {
        if(requiredTool != _type) return;

        if(_toughness >= toughness)
        health -= damage;
        
        OnDamage(sender);

        source.pitch = Random.Range(0.85f, 1.25f);
        source?.Play();

        if(health <= 0)
        {
            OnDestroyed();
        }
    }

    public virtual void OnDamage(GameObject sender)
    {
        
    }

    public virtual void OnDestroyed()
    {
        
    }
}
