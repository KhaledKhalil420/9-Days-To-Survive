using UnityEngine;

public class DynamicMusic : MonoBehaviour
{
    public static DynamicMusic instance;
    public bool toggle = false;

    [SerializeField] private AudioSource chillSource;
    [SerializeField] private AudioSource combatSource;
    [SerializeField] private float fadeSpeed = 2f;

    private void Awake() 
    {
        instance = this;
    }

    public void Start()
    {
        if (chillSource != null && !chillSource.isPlaying) chillSource.Play();
        if (combatSource != null && !combatSource.isPlaying) combatSource.Play();

        chillSource.volume = 1f;
        combatSource.volume = 0f;
    }

    private void Update()
    {
        if (chillSource == null || combatSource == null) return;

        float chillTarget = toggle ? 0f : 1f;
        float combatTarget = toggle ? 1f : 0f;

        chillSource.volume = Mathf.MoveTowards(chillSource.volume, chillTarget, fadeSpeed * Time.deltaTime);
        combatSource.volume = Mathf.MoveTowards(combatSource.volume, combatTarget, fadeSpeed * Time.deltaTime);
    }

    public static void InDanger(bool value)
    {
        instance.toggle = value;
    }
}
