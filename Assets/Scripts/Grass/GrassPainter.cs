using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class GrassPainter : MonoBehaviour
{
    public Mesh mesh;
    MeshFilter filter;

    public Color AdjustedColor;

    [Range(1, 600000)]
    public int grassLimit = 50000;

    private Vector3 lastPosition = Vector3.zero;

    public int toolbarInt = 0;

    public int toolbarIntEdit = 0;

    [SerializeField]
    List<Vector3> positions = new List<Vector3>();
    [SerializeField]
    List<Color> colors = new List<Color>();
    [SerializeField]
    List<int> indicies = new List<int>();
    [SerializeField]
    List<Vector3> normals = new List<Vector3>();
    [SerializeField]
    List<Vector2> length = new List<Vector2>();

    public int i = 0;

    public float sizeWidth = 1f;
    public float sizeLength = 1f;
    public float density = 1f;

    public float normalLimit = 1;

    public float rangeR, rangeG, rangeB;
    public LayerMask hitMask = 1;
    public LayerMask paintMask = 1;
    public float brushSize;
    public float minSpawnHeight;
    public float brushFalloffSize;

    public static GrassPainter instance;

    public float Flow;

    private int flowTimer;

    Vector3 mousePos;

    [HideInInspector]
    public Vector3 hitPosGizmo;

    Vector3 hitPos;

    [HideInInspector]
    public Vector3 hitNormal;

    int[] indi;

    void Awake()
    {
        if (!instance)
            instance = this;
        
        filter = GetComponent<MeshFilter>();
    }

    public void ClearMesh()
    {
        i = 0;
        positions = new List<Vector3>();
        indicies = new List<int>();
        colors = new List<Color>();
        normals = new List<Vector3>();
        length = new List<Vector2>();
        lastPosition = Vector3.zero; // Reset to allow new grass
    }

    public static System.EventHandler OnPaintedGrass;

    void OnDestroy()
    {
    }

    private void OnEnable()
    {
        filter = GetComponent<MeshFilter>();
    }

    // Original method - for backwards compatibility
    public void PaintAtRunTime(Vector3 pos)
    {
        PaintGrassAtPosition(pos);
        FinalizeMesh();
    }

    // Optimized method - paint without building mesh
    public void PaintGrassAtPosition(Vector3 pos)
    {
        RaycastHit terrainHit;
        
        for (int k = 0; k < density; k++)
        {
            float t = 2f * Mathf.PI * Random.Range(0f, 1f);
            float u = Random.Range(0f, 1f) + Random.Range(0f, 1f);
            float r = (u > 1 ? 2 - u : u) * brushSize;

            Vector3 origin = Vector3.zero;
            if (k != 0)
            {
                origin.x += r * Mathf.Cos(t);
                origin.z += r * Mathf.Sin(t);
            }

            Ray ray = new Ray(pos, Vector3.down);
            ray.origin += origin;

            if (Physics.Raycast(ray, out terrainHit, 200f, hitMask.value) &&
                i < grassLimit &&
                terrainHit.normal.y <= (1 + normalLimit) &&
                terrainHit.normal.y >= (1 - normalLimit))
            {
                if ((paintMask.value & (1 << terrainHit.transform.gameObject.layer)) > 0)
                {
                    hitPos = terrainHit.point;
                    hitNormal = terrainHit.normal;

                    if (hitPos.y <= minSpawnHeight)
                        continue;

                    if (k != 0 || Vector3.Distance(terrainHit.point, lastPosition) > brushSize)
                    {
                        var grassPosition = hitPos - transform.position;
                        positions.Add(grassPosition);
                        indicies.Add(i);
                        length.Add(new Vector2(sizeWidth, sizeLength));
                        colors.Add(new Color(
                            AdjustedColor.r + Random.Range(0, 1.0f) * rangeR,
                            AdjustedColor.g + Random.Range(0, 1.0f) * rangeG,
                            AdjustedColor.b + Random.Range(0, 1.0f) * rangeB,
                            1));
                        normals.Add(terrainHit.normal);
                        i++;

                        if (origin == Vector3.zero)
                            lastPosition = hitPos;
                    }
                }
            }
        }
    }

    // Build mesh once after all painting
    public void FinalizeMesh()
    {
        if (positions.Count == 0)
        {
            Debug.LogWarning("FinalizeMesh called but no grass positions exist!");
            return;
        }

        mesh = new Mesh();
        mesh.SetVertices(positions);
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Points, 0);
        mesh.SetUVs(0, length);
        mesh.SetColors(colors);
        mesh.SetNormals(normals);
        filter.mesh = mesh;

        Debug.Log($"Mesh finalized with {positions.Count} grass positions");
        OnPaintedGrass?.Invoke(this, System.EventArgs.Empty);
    }
}