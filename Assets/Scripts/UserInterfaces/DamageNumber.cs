using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class DamageNumber : MonoBehaviour
{
    private TMP_Text tmp;
    private Transform trans;
    private static DamageNumber[] pool = new DamageNumber[50];
    private static int poolIndex;
    private static Camera cam;
    private static Transform camTransform;
    private static DamageNumber prefab;
    private static List<DamageNumber> active = new List<DamageNumber>(50);
    private static bool managerCreated;

    void Awake()
    {
        if (tmp == null) tmp = GetComponentInChildren<TMP_Text>();
        if (trans == null) trans = transform;
        
        if (!managerCreated)
        {
            var manager = new GameObject("DamageNumberManager");
            manager.AddComponent<DamageNumberManager>();
            DontDestroyOnLoad(manager);
            managerCreated = true;
        }
    }

    private void UpdateBillboard()
    {
        trans.rotation = Quaternion.LookRotation(trans.position - camTransform.position);
    }

    public static void Spawn(float damage, Vector3 worldPos)
    {
        if (cam == null)
        {
            cam = Camera.main;
            camTransform = cam.transform;
        }
        if (prefab == null) prefab = Resources.Load<DamageNumber>("DamageNumber");
        
        DamageNumber dn = pool[poolIndex];
        if (dn == null)
        {
            dn = pool[poolIndex] = Instantiate(prefab);
            // Force initialization immediately after instantiate
            dn.tmp = dn.GetComponentInChildren<TMP_Text>();
            dn.trans = dn.transform;
        }
        
        poolIndex = (poolIndex + 1) % pool.Length;
        
        dn.tmp.text = damage.ToString("F0");
        dn.trans.position = worldPos;
        dn.tmp.alpha = 1f;
        dn.gameObject.SetActive(true);
        
        active.Add(dn);
        
        Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.8f, 1.2f), Random.Range(-0.3f, 0.3f));
        
        DOTween.Sequence()
            .Append(dn.trans.DOMove(worldPos + randomOffset, 0.8f).SetEase(Ease.OutQuad))
            .Join(dn.tmp.DOFade(0f, 0.8f).SetEase(Ease.InQuad))
            .OnComplete(() => {
                dn.gameObject.SetActive(false);
                active.Remove(dn);
            })
            .SetRecyclable(true)
            .SetAutoKill(true);
    }

    private class DamageNumberManager : MonoBehaviour
    {
        void LateUpdate()
        {
            if (camTransform == null) return;
            for (int i = 0; i < active.Count; i++)
                active[i].UpdateBillboard();
        }
    }
}