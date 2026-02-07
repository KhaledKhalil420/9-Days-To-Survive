using Sortify;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlayerStat
{
    [SerializeField] internal float max = 100;
    [SerializeField, ReadOnly] internal float current;
    [SerializeField] internal float modifyRate = 1;
    [SerializeField] internal bool canModifyRate = true;
    [SerializeField] internal bool isDecaying = false;

    [SerializeField] private float delay = 5; 
    [SerializeField, ReadOnly] private float delayTimer = 0;

    [Space(3)]

    [SerializeField] private Slider slider;
    [SerializeField] private float sliderLerp = 1;
    [SerializeField] private TMP_Text text;

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
    
    private void Restore()
    {
        if(isDecaying)
        {
            current = Mathf.Clamp(current - modifyRate * Time.deltaTime, 0, max);
            return;
        }

        current = Mathf.Clamp(current + modifyRate * Time.deltaTime, 0, max);
    }

    public void TickUi()
    {
        slider.maxValue = max;
        slider.value = Mathf.Lerp(slider.value, current, sliderLerp * Time.deltaTime);

        text.text = Mathf.Round(current) + " / " + Mathf.Round(max);
    }
}

public class PlayerStats : MonoBehaviour, IDamagable
{
    [Header("Health")]
    [SerializeField] internal PlayerStat health = new();


    [Header("Hunger")]
    [SerializeField] internal PlayerStat hunger = new();

    [Header("Stamina")]
    [SerializeField] internal PlayerStat stamina = new();
    [SerializeField] private float staminaConsumption = 1;
    [SerializeField] private PlayerMovement movement;
    

    void Start()
    {
        health.Initialize();
        hunger.Initialize();
        stamina.Initialize();

        movement.OnJump += OnJump;
    }

    void Update()
    {
        health.Tick();
        hunger.Tick();
        stamina.Tick(); 

        health.TickUi();
        hunger.TickUi();
        stamina.TickUi();      

        HandleStamina();
        HandleHunger();
        HandleHealth();
    }

    #region Health

    private void HandleHealth()
    {
        //Lost.
        if(health.current <= 0)
        {
            GameManager.Instance.PlayerLost();
        }
    }

    public void Damage(float damage)
    {
        health.Modify(-damage);
    }

    public void Heal(float heal)
    {
        health.Modify(heal);
    }

    #endregion

    #region Stamina

    private void HandleStamina()
    {
        movement.canRun = stamina.current > 0;
        movement.canJump = stamina.current > 0;

        if(movement.isRunning)
        {
            stamina.Modify(-staminaConsumption * Time.deltaTime);
        }
    }

    public void OnJump()
    {
        stamina.Modify(-staminaConsumption);
    }

    #endregion

    #region Hunger
    
    private void HandleHunger()
    {
        stamina.canModifyRate = hunger.current > 0;
    }

    public void Eat(float satiation)
    {
        hunger.Modify(satiation);
    }

    #endregion
}