using TMPro;
using UnityEngine;

public class TMPWobble : MonoBehaviour
{
    public TMP_Text textMesh;
    public float wobbleSpeed = 2f;
    public float wobbleAmount = 5f;
    public bool wobbleX = false;
    public bool wobbleY = true;

    void Start()
    {
        if (textMesh == null)
            textMesh = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (textMesh == null) return;

        textMesh.ForceMeshUpdate();
        var textInfo = textMesh.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            int materialIndex = charInfo.materialReferenceIndex;
            var verts = textInfo.meshInfo[materialIndex].vertices;
            
            float xWobble = wobbleX ? Mathf.Sin(Time.time * wobbleSpeed + i * 0.5f) * wobbleAmount : 0;
            float yWobble = wobbleY ? Mathf.Cos(Time.time * wobbleSpeed + i * 0.5f) * wobbleAmount : 0;
            Vector3 wobble = new Vector3(xWobble, yWobble, 0);

            Vector3 offset = charInfo.topLeft;
            
            for (int j = 0; j < 4; j++)
            {
                verts[vertexIndex + j] = verts[vertexIndex + j] - offset + wobble + offset;
            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            textMesh.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}