using UnityEngine;
using UnityEngine.AI;

public enum BreakableType {Pickaxe, Axe, Else, Buildings}
public class Breakable : MonoBehaviour, IBreakable
{
    [SerializeField] protected AdvancedAudioSource source;
    [SerializeField] private BreakableType requiredTool;
    [SerializeField] private int toughness, health;
    [SerializeField] protected Item item;
    private GameObject sender;

    private void Start()
    {
        source = GetComponent<AdvancedAudioSource>();
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
            OnDestroyed();
        }
    }

    public virtual void OnDamage(int damage, GameObject sender)
    {
        
    }

    public virtual void OnDestroyed()
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
