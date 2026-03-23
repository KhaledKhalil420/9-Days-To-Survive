using Sortify;
using UnityEngine;

public class StatusEffect : MonoBehaviour
{
    public StatusEffectData data;
    [ReadOnly] public StatusEffectTarget target;

    [EditorChangeable] public float effectTime = 1;
    [EditorChangeable] public float effectMaxTime = 3;
    [SerializeField, ReadOnly] private float effectTimer = 0; 

    public int effectStrength = 1;
    
    public void InitializeStatus(bool AddTo = false, StatusEffectTarget _target = null)
    {
        if(AddTo)
        {
            effectTime += effectTime / 2;
            effectTime = Mathf.Max(effectMaxTime);
            return;
        }

        target = _target;
        InitializeEffect();
    }

    public void UpdateStatus()
    {
        effectTimer += Time.deltaTime;

        if(effectTimer > effectTime)
        {
            Expire();
        }

        UpdateEffect();
    }

    public void Expire()
    {
        effectTimer = 1000;
        target.statusEffects.Remove(this);
        Destroy(gameObject);
    }

    protected virtual void InitializeEffect()
    {
        
    }

    protected virtual void UpdateEffect()
    {
        
    }
}
