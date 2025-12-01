using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

[System.Serializable]
public class IconGenerator_Positions
{
    [Header("Tools")]
    public Vector3 positionTool = new Vector3(-0.3f, -0.9f, 0); 
    public Vector3 rotationTool = new Vector3(15, 90, 0);

    [Header("Items")]
    public Vector3 positionItem = Vector3.zero;
    public Vector3 rotationItem = new Vector3(55, 0, -35);
}

public class IconGenerator : MonoBehaviour
{
    public ItemData[] allInGameItems;
    public Camera renderCamera;
    public string saveTo;

    [Header("Background remover")]
    public Color colorToRemove;
    public float tolerance = 0.1f;

    private GameObject itemInstance;

    public IconGenerator_Positions presets;

    public void Load()
    {
        allInGameItems = Resources.LoadAll<ItemData>("");
    }

    public void SpawnObject()
    {
        if (itemInstance != null) DestroyImmediate(itemInstance);
        if (allInGameItems.Length > 0)
        {
            var item = allInGameItems[0];
            itemInstance = Instantiate(item.prefab, Vector3.zero, Quaternion.identity);
            itemInstance.tag = "Finish";
        }
    }

    public async void SaveIcon()
    {
        // 1) render to transparent RT
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = new Color(colorToRemove.r, colorToRemove.g, colorToRemove.b, 0);

        var rt = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
        renderCamera.targetTexture = rt;
        renderCamera.Render();
        RenderTexture.active = rt;

        var tex = new Texture2D(512, 512, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        ApplyTransparencyMask(tex);

        // 2) write PNG on background thread
        byte[] bytes = tex.EncodeToPNG();
        string fullPath = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - 6), saveTo, allInGameItems[0].Name + "_Icon.png");
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        await Task.Run(() => File.WriteAllBytes(fullPath, bytes));

        // 3) cleanup
        RenderTexture.active = null;
        renderCamera.targetTexture = null;
        DestroyImmediate(rt);

#if UNITY_EDITOR
        // 4) refresh & reimport as Sprite
        string assetPath = fullPath.Replace(Application.dataPath, "Assets");
        AssetDatabase.Refresh();  // make Unity see the new file
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }

        // 5) assign Sprite on the item data
        allInGameItems[0].sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
#endif
    }

    public void DeleteCurrent()
    {
        DestroyImmediate(GameObject.FindWithTag("Finish"));
        itemInstance = null;
    }

    public void SetPositionAsTool()
    {
        
        itemInstance.transform.position = presets.positionTool;
        itemInstance.transform.eulerAngles = presets.rotationTool;
    }

    public void SetPositionAsItem()
    {
        itemInstance.transform.position = presets.positionItem;
        itemInstance.transform.eulerAngles = presets.rotationItem;
    }

    private void ApplyTransparencyMask(Texture2D texture)
    {
        var pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (Mathf.Abs(pixels[i].r - colorToRemove.r) < tolerance &&
                Mathf.Abs(pixels[i].g - colorToRemove.g) < tolerance &&
                Mathf.Abs(pixels[i].b - colorToRemove.b) < tolerance)
            {
                pixels[i].a = 0;
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(IconGenerator))]
public class IconGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var g = (IconGenerator)target;
        if (GUILayout.Button("Load all Items")) g.Load();
        if (GUILayout.Button("Generate Icon")) g.SpawnObject();
        if (GUILayout.Button("Save Icon")) g.SaveIcon();
        if (GUILayout.Button("Remove current object")) g.DeleteCurrent();
        if (GUILayout.Button("Set position as tool")) g.SetPositionAsTool();
        if (GUILayout.Button("Set position as item")) g.SetPositionAsItem();
    }
}
#endif
