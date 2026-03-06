using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player")]
    public static Player Player => Instance?.player;
    private Player player;
    public List<Item> starterItems;

    [Header("Death")]
    [SerializeField] private Volume volume;
    [SerializeField] private CanvasGroup group;

    #region Unity

    private void Awake()
    {
        Instance = this;
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void Start()
    {
        #if  UNITY_EDITOR

        #else
        group.alpha = 1;
        group.DOFade(0, 5);
        #endif
        GivePlayerStarterItems();
    }

    #endregion

    #region Player

    private void GivePlayerStarterItems()
    {
        #if UNITY_EDITOR
        foreach(Item item in starterItems)
            player.GetComponent<PlayerInventory>().GiveItem(item);

        #else 
        
        foreach(Item item in starterItems)
            Destroy(item.gameObject);

        #endif
    }

    public void PlayerLost()
    {
        //TEMP.. RESTART GAME
        DOTween.KillAll(false);
        
        DOVirtual.Float(volume.weight, 1, 1f, value => volume.weight = value);
        Sequence eseq = DOTween.Sequence();
        Transform cam = GameObject.FindWithTag("MainCamera").transform;
        AudioManager.Instance.PlaySound("PlayerDeath");
        eseq.Append(cam.transform.DOLocalMove(cam.localPosition + Vector3.back * 1.5f - new Vector3(0, 1, 0), 0.5f))
            .Join(cam.transform.DOLocalRotate(new Vector3(-65, 0, 0), 2f));
            
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0, 2f)
            .SetUpdate(true);

        DOTween.To(() => Time.fixedDeltaTime, x => Time.fixedDeltaTime = x, 0, 2f)
            .SetUpdate(true);

        player.Disable();

        AudioManager.Instance.SlowDown();
        StartCoroutine(AudioManager.Instance.FadeOutLowpass());

        UiManager.ToggleUi(true);
        UiManager.CloseAll();
        
        DOVirtual.DelayedCall(5f, () => { AudioManager.Instance.FadeOut(false); group.DOFade(1f, 2.5f).SetUpdate(true).OnComplete(() => { SceneManager.LoadScene(0); Time.timeScale = 1f; Time.fixedDeltaTime = 0.01f; }); }).SetUpdate(true);
    }

    private void Update()
    {
        #if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.F1))
        {
            Player.stats.health.max = 10000;
            Player.stats.health.modifyRate = 10000;

            Player.stats.stamina.max = 10000;
            Player.stats.stamina.modifyRate = 10000;

            Player.inventory.speedBonus *= 1.5f;

            BuildingManager.Instance.buildLimitPoints = 1000;
            BuildingManager.Instance.extraBuildingHealth = 1000;
        }
        #endif
    }   

    #endregion
}