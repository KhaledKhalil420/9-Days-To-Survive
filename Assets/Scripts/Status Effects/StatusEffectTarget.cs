using System.Collections.Generic;
using UnityEngine;

public class StatusEffectTarget : MonoBehaviour
{
    public List<StatusEffect> statusEffects = new List<StatusEffect>();

    public void AddStatusEffect(StatusEffect statusEffect)
    {
        //Increase effect duration if this already has it
        foreach (StatusEffect status in statusEffects)
        {
            if(status.GetType() == statusEffect.GetType())
            {
                statusEffect.InitializeStatus(true);
                return;
            }
        }
        
        //Create new status effect and add it to the list
        StatusEffect statusObject = Instantiate(statusEffect, transform);
        statusEffects.Add(statusObject);
        statusObject.InitializeStatus(false, this);
    }

    private void Update()
    {
        foreach (StatusEffect status in statusEffects)
        {
            status.UpdateStatus();
        }
    }
}
