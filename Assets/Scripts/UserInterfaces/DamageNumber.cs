using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamageNumber : MonoBehaviour
{
    private TMP_Text tmp;
    private Transform trans;
    private static DamageNumber[] pool = new DamageNumber[50];
    private static int poolIndex;
    private static Camera cam;
    private static DamageNumber prefab;

    void Start()
    {
        tmp = GetComponentInChildren<TMP_Text>();
        trans = transform;
    }

    void LateUpdate()
    {
        if (gameObject.activeSelf && cam != null)
            trans.rotation = Quaternion.LookRotation(trans.position - cam.transform.position);
    }

    public static void Spawn(float damage, Vector3 worldPos)
    {
        if (cam == null) cam = Camera.main;
        if (prefab == null) prefab = Resources.Load<DamageNumber>("DamageNumber");
        
        DamageNumber dn = pool[poolIndex];
        if (dn == null) dn = pool[poolIndex] = Instantiate(prefab);
        
        poolIndex = (poolIndex + 1) % pool.Length;
        
        dn.tmp.text = damage.ToString("F0");
        dn.trans.position = worldPos;
        dn.tmp.alpha = 1f;
        dn.gameObject.SetActive(true);
        
        Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.8f, 1.2f), Random.Range(-0.3f, 0.3f));
        
        DOTween.Sequence()
            .Append(dn.trans.DOMove(worldPos + randomOffset, 0.8f).SetEase(Ease.OutQuad))
            .Join(dn.tmp.DOFade(0f, 0.8f).SetEase(Ease.InQuad))
            .OnComplete(() => dn.gameObject.SetActive(false))
            .SetRecyclable(true);
    }
}