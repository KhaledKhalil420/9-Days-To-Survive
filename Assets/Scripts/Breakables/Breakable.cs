using UnityEngine;
using UnityEngine.AI;

public enum BreakableType {Pickaxe, Axe, Else, Buildings}
public class Breakable : MonoBehaviour, IBreakable
{
    [SerializeField] protected AdvancedAudioSource source;
    [SerializeField] private BreakableType requiredTool;
    [SerializeField] protected int toughness, health;
    protected int fullHealth;
    [SerializeField] protected Item item;
    private GameObject sender;

    private void Start()
    {
        source = GetComponent<AdvancedAudioSource>();
        fullHealth = health;
    }

    public void Damage(GameObject sender, int damage, BreakableType _type, int _toughness)
    {
        if(requiredTool != _type) return;

        if(_toughness >= toughness)
        health -= 1;
        
        OnDamage(damage, sender);

        source.Play(true, 0.9f, 1.25f);

        if(health <= 0)
        {
            OnDestroyed(sender);
        }
    }

    public virtual void OnDamage(int damage, GameObject sender)
    {
        
    }

    public virtual void OnDestroyed(GameObject sender)
    {
        
    }

    public void DisableBreakable()
    {
        GetComponent<Renderer>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        GetComponent<NavMeshObstacle>().enabled = false;
    }
}
