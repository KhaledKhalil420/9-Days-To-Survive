using TMPro;
using UnityEngine;
using System.Collections;

public class TMPTypewriter : MonoBehaviour
{
    public TMP_Text textMesh;
    public float typeSpeed = 0.05f;
    string fullText;

    void Start()
    {
        fullText = textMesh.text;
        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        textMesh.text = "";
        foreach (char c in fullText)
        {
            textMesh.text += c;
            yield return new WaitForSeconds(typeSpeed / 100);
        }
    }
}