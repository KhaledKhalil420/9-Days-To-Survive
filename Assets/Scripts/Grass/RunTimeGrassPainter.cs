using System.Collections.Generic;
using UnityEngine;

public class RunTimeGrassPainter : MonoBehaviour
{
    private GrassPainter painter;
    public int points = 4;
    public float spread = 2f;
    public float regenDistance = 2f;

    private Vector3 lastPaintPos;

    // Store generated grass positions
    private List<Vector3> grassPositions = new List<Vector3>();

    void Start()
    {
        painter = GrassPainter.instance;

        if (!painter) return;
        GenerateGrassPositions();
        Repaint();
    }

    void Update()
    {
        // Only repaint when player moved enough
        if (Vector3.Distance(transform.position, lastPaintPos) > regenDistance)
        {
            Repaint();
            lastPaintPos = transform.position;
        }
    }

    void GenerateGrassPositions()
    {
        grassPositions.Clear();

        for (int x = -points / 2; x < points / 2; x++)
        for (int z = -points / 2; z < points / 2; z++)
        {
            // Add some small random jitter so it doesn't look like a grid
            float jitterX = Random.Range(-spread * 0.4f, spread * 0.4f);
            float jitterZ = Random.Range(-spread * 0.4f, spread * 0.4f);

            Vector3 offset = new Vector3(x * spread + jitterX, 10f, z * spread + jitterZ);
            grassPositions.Add(offset);
        }
    }

    void Repaint()
    {
        painter.ClearMesh();

        foreach (Vector3 offset in grassPositions)
        {
            Vector3 paintPos = transform.position + offset;

            // Optionally sample height from terrain or your marching cubes mesh
            paintPos.y = SampleHeight(paintPos);

            painter.PaintAtRunTime(paintPos);
        }
    }

    float SampleHeight(Vector3 worldPos)
    {
        // Replace this with your terrain/marching cubes height sampling
        Ray ray = new Ray(worldPos + Vector3.up * 50f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            return hit.point.y;

        return worldPos.y;
    }
}
