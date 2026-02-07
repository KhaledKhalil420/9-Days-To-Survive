using TMPro;
using UnityEngine;

public class TMPScalePulse : MonoBehaviour
{
    public TMP_Text textMesh;
    public float pulseSpeed = 2f;
    public float scaleAmount = 0.2f;

    void Update()
    {
        textMesh.ForceMeshUpdate();
        var textInfo = textMesh.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            Vector3 center = (verts[charInfo.vertexIndex] + verts[charInfo.vertexIndex + 2]) / 2;
            float scale = 1 + Mathf.Sin(Time.time * pulseSpeed + i * 0.3f) * scaleAmount;

            for (int j = 0; j < 4; j++)
            {
                verts[charInfo.vertexIndex + j] = center + (verts[charInfo.vertexIndex + j] - center) * scale;
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