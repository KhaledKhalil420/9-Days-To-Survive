using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TMPRainbow : MonoBehaviour
{
    public TMP_Text textComponent;
    [SerializeField] private Gradient gradient;
    [SerializeField] private float scrollSpeed = 1f;

    private float time;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    void Update()
    {
        time += Time.deltaTime * scrollSpeed;
        textComponent.ForceMeshUpdate();

        var textInfo = textComponent.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            var colors = textInfo.meshInfo[materialIndex].colors32;

            float t = Mathf.Repeat(time + i * 0.05f, 1f);
            Color color = gradient.Evaluate(t);

            colors[vertexIndex + 0] = color;
            colors[vertexIndex + 1] = color;
            colors[vertexIndex + 2] = color;
            colors[vertexIndex + 3] = color;
        }

        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
}