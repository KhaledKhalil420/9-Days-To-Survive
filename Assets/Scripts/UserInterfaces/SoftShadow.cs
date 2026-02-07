using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Soft Shadow", 80)]
public class SoftShadow : BaseMeshEffect
{
    [SerializeField]
    private Color shadowColor = new Color(0f, 0f, 0f, 0.5f);

    [SerializeField]
    private Vector2 shadowOffset = new Vector2(2f, -2f);

    [SerializeField]
    [Range(2, 10)]
    private int softness = 5;

    [SerializeField]
    [Range(0.1f, 3f)]
    private float blurSpread = 1f;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        List<UIVertex> verts = new List<UIVertex>();
        vh.GetUIVertexStream(verts);

        int originalCount = verts.Count;

        // Add multiple shadow passes
        for (int i = softness - 1; i >= 0; i--)
        {
            float progress = (float)i / (softness - 1);
            
            // Calculate offset for this layer
            float layerOffsetX = shadowOffset.x * progress;
            float layerOffsetY = shadowOffset.y * progress;
            
            // Calculate alpha falloff
            float alpha = shadowColor.a * (1f - progress * 0.7f) / softness;
            Color32 layerColor = shadowColor;
            layerColor.a = (byte)(alpha * 255f);

            // Add blur spread
            float spreadX = Random.Range(-blurSpread, blurSpread) * progress;
            float spreadY = Random.Range(-blurSpread, blurSpread) * progress;

            // Duplicate vertices for this shadow layer
            for (int v = 0; v < originalCount; v++)
            {
                UIVertex vertex = verts[v];
                vertex.position += new Vector3(layerOffsetX + spreadX, layerOffsetY + spreadY, 0);
                vertex.color = layerColor;
                verts.Add(vertex);
            }
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(verts);
    }
}