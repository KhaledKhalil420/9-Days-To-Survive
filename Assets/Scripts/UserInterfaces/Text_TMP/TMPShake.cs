using TMPro;
using UnityEngine;

public class TMPShake : MonoBehaviour
{
    public TMP_Text textMesh;
    public float shakeAmount = 2f;
    public float shakeSpeed = 1f;
    public Vector3 letterOffset;

    void Update()
    {
        textMesh.ForceMeshUpdate();
        var textInfo = textMesh.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            Vector3 shake = new Vector3(
                Mathf.PerlinNoise(Time.time * shakeSpeed + i, 0) * 2 - 1,
                Mathf.PerlinNoise(0, Time.time * shakeSpeed + i) * 2 - 1,
                0
            ) * shakeAmount;

            for (int j = 0; j < 4; j++)
            {
                verts[charInfo.vertexIndex + j] += shake + letterOffset;
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