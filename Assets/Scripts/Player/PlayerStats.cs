using Sortify;
using UnityEngine;

[System.Serializable]
public class PlayerStat
{
    [SerializeField] internal float max = 100;
    [SerializeField, ReadOnly] internal float current;
    [SerializeField] private float modifyRate = 1;
    [SerializeField] internal bool canModifyRate = true;
    [SerializeField] internal bool isDecaying = false;

    [SerializeField] private float delay = 5; 
    [SerializeField, ReadOnly] private float delayTimer = 0;

    public void Initialize()
    {
        current = max;
    }
    
    public void Tick()
    {
        if (!canModifyRate || (!isDecaying && current >= max))
            return;

        if (delayTimer > delay)
            Restore();

        else
        delayTimer += Time.deltaTime;
    }

    public void Modify(float value)
    {
        delayTimer = 0;

        current += value;
        current = Mathf.Clamp(current, 0, max);
    }
    
    void Restore()
    {
        if(isDecaying)
        {
            current = Mathf.Clamp(current - modifyRate * Time.deltaTime, 0, max);
            return;
        }

        current = Mathf.Clamp(current + modifyRate * Time.deltaTime, 0, max);
    }
}

public class PlayerStats : MonoBehaviour, IDamagable
{
    [Header("Health")]
    [SerializeField] private PlayerStat health = new();

    [Header("Hunger")]
    [SerializeField] private PlayerStat hunger = new();
    [SerializeField] private float runningStaminaPercentange = 25;  

    [Header("Stamina")]
    [SerializeField] private PlayerStat stamina = new();
    [SerializeField] private float runningStaminaConsumption = 1;
    [SerializeField] private PlayerMovement movement;

    void Start()
    {
        health.Initialize();
        hunger.Initialize();
        stamina.Initialize();
    }

    void Update()
    {
        health.Tick();
        hunger.Tick();
        stamina.Tick(); 

        HandleStamina();
        HandleHunger();
    }

    #region Health

    public void Damage(int damage)
    {
        health.Modify(-damage);
    }

    public void Heal(int heal)
    {
        health.Modify(heal);
    }

    public void HealthUi()
    {
        
    }

    #endregion

    #region Stamina

    public void HandleStamina()
    {
        movement.canRun = stamina.current > 0;

        if(movement.isRunning)
        {
            stamina.Modify(-runningStaminaConsumption * Time.deltaTime);
        }
    }

    public void StaminaUi()
    {
        
    }

    #endregion

    #region Hunger
    
    public void HandleHunger()
    {
        stamina.canModifyRate = hunger.current > 0;
    }

    public void Eat(float satiation)
    {
        hunger.Modify(satiation);
    }

    public void HungerUi()
    {
        
    }

    #endregion
}