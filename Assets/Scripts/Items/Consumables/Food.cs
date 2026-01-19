using DG.Tweening;
using UnityEngine;

public class Food : Consumables
{
    [SerializeField] private float health, satiation;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip eatingSound, ateSound;
    private Tween eatingSoundTween;

    public override void OnStartConsume()
    {
        source.loop = true;
        source.volume = 0.6f;
        source.pitch = Random.Range(0.9f, 1.15f);
        source.PlayOneShot(eatingSound);

        eatingSoundTween.Kill();
    }

    public override void OnStopConsume()
    {
        source.Stop();
        eatingSoundTween.Kill();
        eatingSoundTween = DOVirtual.Float(source.volume, 0f , 0.3f, x => source.volume = x).SetAutoKill(true);   
    }

    public override void OnConsumed()
    {
        playerStats.Heal(health);
        playerStats.Eat(satiation);
        playerStats.stamina.Modify(satiation * 4);

        source.loop = false;
        source.PlayOneShot(ateSound);
    }
}
