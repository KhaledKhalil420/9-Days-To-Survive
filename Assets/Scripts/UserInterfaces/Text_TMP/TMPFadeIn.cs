using TMPro;
using UnityEngine;

public class TMPFadeIn : MonoBehaviour
{
    public TMP_Text textMesh;
    public float fadeSpeed = 1f;
    float alpha = 0;

    void Update()
    {
        alpha += Time.deltaTime * fadeSpeed;
        textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, Mathf.Clamp01(alpha));
    }
}