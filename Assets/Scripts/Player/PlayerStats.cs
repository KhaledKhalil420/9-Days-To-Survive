using DG.Tweening;
using EZCameraShake;
using Sortify;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[System.Serializable]
public class PlayerStat
{
    [SerializeField] internal float max = 100;
    [SerializeField, Sortify.ReadOnly] internal float current;
    [SerializeField] internal float modifyRate = 1;
    [SerializeField] internal bool canModifyRate = true;
    [SerializeField] internal bool isDecaying = false;

    [SerializeField] private float delay = 5; 
    [SerializeField, Sortify.ReadOnly] private float delayTimer = 0;

    [Space(3)]

    [SerializeField] private Slider slider;
    [SerializeField] private bool roundSlider = false;
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
        float v = roundSlider ? sliderLerp * 100 : sliderLerp;
        slider.value = Mathf.Lerp(slider.value, current, v * Time.deltaTime);

        text.text = Mathf.Round(current) + " / " + Mathf.Round(max);
    }
}

public class PlayerStats : MonoBehaviour, IDamagable
{
    [Header("Health")]
    [SerializeField] internal PlayerStat health = new();
    [SerializeField] private Volume damagedVolume;


    [Header("Hunger")]
    [SerializeField] internal PlayerStat hunger = new();

    [Header("Stamina")]
    [SerializeField] internal PlayerStat stamina = new();
    [SerializeField] internal float staminaConsumption = 1;
    [SerializeField] internal bool jumpingConsumingStamina = true;
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

        DamageFeedback();
    }

    private void DamageFeedback()
    {
        AudioManager.Instance.PlaySound("PlayerDamaged", 0.9f, 1.2f);
        CameraShaker.Instance.ShakeOnce(5, 1, 0.1f, 0.1f);
        DOVirtual.Float(damagedVolume.weight, 1, 0.1f,value => damagedVolume.weight = value).OnComplete(() => DOVirtual.Float(damagedVolume.weight, 0, 1f,value => damagedVolume.weight = value));
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
        if(jumpingConsumingStamina)
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