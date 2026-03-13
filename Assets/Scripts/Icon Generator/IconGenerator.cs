using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

public class IconGenerator : MonoBehaviour
{
    #region References
    [Header("References")]
    public Camera renderCamera;
    public string saveTo = "Assets/Icons";
    #endregion

    #region Items
    [Header("Items")]
    public ItemData[] allInGameItems;
    public int currentIndex = 0;
    #endregion

    #region Render Settings
    [Header("Render Settings")]
    public IconResolution resolution = IconResolution.x512;
    public bool skipExisting = true;
    #endregion

    #region Background Removal
    [Header("Background Removal")]
    public Color colorToRemove = Color.grey;
    public float tolerance = 0.1f;
    #endregion

    #region Auto Focus
    [Header("Auto Focus")]
    public float focusDistanceMultiplier = 1.5f;
    public Vector3 focusOffset = Vector3.zero;
    #endregion

    #region Presets
    [Header("Presets")]
    public List<IconPreset> presets = new List<IconPreset>();
    public int activePreset = 0;
    #endregion

    private GameObject itemInstance;

    #region Public API

    public void Load()
    {
        allInGameItems = Resources.LoadAll<ItemData>("");
        currentIndex = 0;
        Debug.Log($"[IconGenerator] Loaded {allInGameItems.Length} items.");
    }

    public void SpawnCurrent()
    {
        if (allInGameItems == null || allInGameItems.Length == 0) return;
        DeleteCurrent();

        var item = allInGameItems[currentIndex];
        if (item?.prefab == null) return;

        itemInstance = Instantiate(item.prefab, Vector3.zero, Quaternion.identity);
        itemInstance.tag = "Finish";
        ApplyActivePreset();
    }

    public void NextItem()
    {
        if (allInGameItems == null || allInGameItems.Length == 0) return;
        currentIndex = (currentIndex + 1) % allInGameItems.Length;
        SpawnCurrent();
    }

    public void PrevItem()
    {
        if (allInGameItems == null || allInGameItems.Length == 0) return;
        currentIndex = (currentIndex - 1 + allInGameItems.Length) % allInGameItems.Length;
        SpawnCurrent();
    }

    public async void SaveCurrent()
    {
        if (itemInstance == null) SpawnCurrent();
        await RenderAndSave(currentIndex);
        DeleteCurrent();
#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
#endif
    }

    public async void SaveAll()
    {
        if (allInGameItems == null || allInGameItems.Length == 0)
        {
            Debug.LogWarning("[IconGenerator] No items loaded.");
            return;
        }

        int saved = 0;
        int skipped = 0;

        for (int i = 0; i < allInGameItems.Length; i++)
        {
            currentIndex = i;
            SpawnCurrent();
            await Task.Yield();

            bool wasSkipped = skipExisting && File.Exists(GetFullPath(allInGameItems[i].Name));
            await RenderAndSave(i);
            DeleteCurrent();

            if (wasSkipped) skipped++; else saved++;
        }

#if UNITY_EDITOR
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
#endif
        Debug.Log($"[IconGenerator] Batch done — Saved: {saved}, Skipped: {skipped}");
    }

    public void ApplyActivePreset()
    {
        if (presets == null || presets.Count == 0 || itemInstance == null) return;
        activePreset = Mathf.Clamp(activePreset, 0, presets.Count - 1);

        var p = presets[activePreset];
        itemInstance.transform.position = p.position;
        itemInstance.transform.eulerAngles = p.rotation;
        itemInstance.transform.localScale = p.scale;
    }

    public void DeleteCurrent()
    {
        var existing = GameObject.FindWithTag("Finish");
        if (existing != null) DestroyImmediate(existing);
        itemInstance = null;
    }

    public void AutoFocusCamera()
    {
        if (itemInstance == null || renderCamera == null) return;
        var bounds = GetCombinedBounds(itemInstance);
        float dist = bounds.size.magnitude * focusDistanceMultiplier;
        renderCamera.transform.position = bounds.center - renderCamera.transform.forward * dist + focusOffset;
    }

    #endregion

    #region Private

    private async Task RenderAndSave(int index)
    {
        var item = allInGameItems[index];
        if (item == null) return;

        string fullPath = GetFullPath(item.Name);

        if (skipExisting && File.Exists(fullPath))
        {
            Debug.Log($"[IconGenerator] Skipped (exists): {item.Name}");
            return;
        }

        int res = (int)resolution;
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = new Color(colorToRemove.r, colorToRemove.g, colorToRemove.b, 0);

        var rt = new RenderTexture(res, res, 24, RenderTextureFormat.ARGB32);
        renderCamera.targetTexture = rt;
        renderCamera.Render();
        RenderTexture.active = rt;

        var tex = new Texture2D(res, res, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, res, res), 0, 0);
        ApplyTransparencyMask(tex);

        byte[] bytes = tex.EncodeToPNG();
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        await Task.Run(() => File.WriteAllBytes(fullPath, bytes));

        RenderTexture.active = null;
        renderCamera.targetTexture = null;
        DestroyImmediate(rt);
        DestroyImmediate(tex);

#if UNITY_EDITOR
        string assetPath = fullPath.Replace(Application.dataPath, "Assets");
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite != null)
        {
            item.sprite = sprite;
            EditorUtility.SetDirty(item);
        }
        else
        {
            Debug.LogWarning($"[IconGenerator] Could not load sprite at: {assetPath}");
        }

        Debug.Log($"[IconGenerator] Saved: {item.Name}");
#endif
    }

    private void ApplyTransparencyMask(Texture2D texture)
    {
        var pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (Mathf.Abs(pixels[i].r - colorToRemove.r) < tolerance &&
                Mathf.Abs(pixels[i].g - colorToRemove.g) < tolerance &&
                Mathf.Abs(pixels[i].b - colorToRemove.b) < tolerance)
                pixels[i].a = 0;
        }
        texture.SetPixels(pixels);
        texture.Apply();
    }

    private string GetFullPath(string itemName)
    {
        string root = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
        return Path.Combine(root, saveTo, itemName + "_Icon.png");
    }

    private Bounds GetCombinedBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one);
        var b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        return b;
    }

    #endregion
}

public enum IconResolution { x128 = 128, x256 = 256, x512 = 512, x1024 = 1024 }

[System.Serializable]
public class IconPreset
{
    public string name = "New Preset";
    public Vector3 position = Vector3.zero;
    public Vector3 rotation = new Vector3(55, 0, -35);
    public Vector3 scale = Vector3.one;
}

#if UNITY_EDITOR
[CustomEditor(typeof(IconGenerator))]
public class IconGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var g = (IconGenerator)target;

        EditorGUILayout.Space(10);

        // Current item info
        if (g.allInGameItems != null && g.allInGameItems.Length > 0)
        {
            string itemName = g.allInGameItems[g.currentIndex]?.Name ?? "null";
            EditorGUILayout.LabelField($"[ {g.currentIndex + 1} / {g.allInGameItems.Length} ]  {itemName}", EditorStyles.boldLabel);
        }

        EditorGUILayout.Space(4);

        // Load
        if (GUILayout.Button("🔄  Load All Items", GUILayout.Height(28))) g.Load();

        EditorGUILayout.Space(6);

        // Navigation
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("◀  Prev", GUILayout.Height(26))) g.PrevItem();
        if (GUILayout.Button("Spawn Current", GUILayout.Height(26))) g.SpawnCurrent();
        if (GUILayout.Button("Next  ▶", GUILayout.Height(26))) g.NextItem();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);

        // Preset buttons
        if (g.presets != null && g.presets.Count > 0)
        {
            EditorGUILayout.LabelField("Presets", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < g.presets.Count; i++)
            {
                GUI.backgroundColor = (g.activePreset == i) ? new Color(0.4f, 0.9f, 1f) : Color.white;
                if (GUILayout.Button(g.presets[i].name, GUILayout.Height(24)))
                {
                    g.activePreset = i;
                    g.ApplyActivePreset();
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(6);

        // Camera
        if (GUILayout.Button($"🎯  Auto-Focus Camera  (x{g.focusDistanceMultiplier})", GUILayout.Height(26))) g.AutoFocusCamera();

        EditorGUILayout.Space(6);

        // Save
        GUI.backgroundColor = new Color(0.6f, 1f, 0.65f);
        if (GUILayout.Button("💾  Save Current Icon", GUILayout.Height(30))) g.SaveCurrent();

        GUI.backgroundColor = new Color(0.3f, 0.85f, 0.45f);
        if (GUILayout.Button("💾  Save ALL Icons", GUILayout.Height(30))) g.SaveAll();

        GUI.backgroundColor = Color.white;
        EditorGUILayout.Space(6);

        // Cleanup
        GUI.backgroundColor = new Color(1f, 0.55f, 0.55f);
        if (GUILayout.Button("🗑  Remove Current Object", GUILayout.Height(26))) g.DeleteCurrent();
        GUI.backgroundColor = Color.white;
    }
}
#endif