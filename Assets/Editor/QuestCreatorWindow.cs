using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class QuestCreatorWindow : EditorWindow
{
    private const string TEMPLATES_PATH = "Assets/Prefabs/Quests/Templates";
    private const string PREFABS_PATH = "Assets/Prefabs/Quests";
    private const string DATA_PATH = "Assets/Resources/QuestsData";

    private List<GameObject> templates = new();
    private string[] templateNames = Array.Empty<string>();
    private int selectedTemplate;

    private List<Type> questTypes = new();
    private string[] questTypeNames = Array.Empty<string>();
    private int selectedType;

    private string questName = "New Quest";
    private string description = "Description...";
    private Sprite sprite;

    private QuestManager questManager;

    [MenuItem("Khaled/Quest Creator")]
    public static void Open()
    {
        var w = GetWindow<QuestCreatorWindow>("Quest Creator");
        w.minSize = new Vector2(380, 480);
    }

    private void OnEnable()
    {
        RefreshTemplates();
        RefreshTypes();
        questManager = FindObjectOfType<QuestManager>();
    }

    private void RefreshTemplates()
    {
        templates = AssetDatabase.FindAssets("t:Prefab", new[] { TEMPLATES_PATH })
            .Select(guid => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(go => go != null && go.GetComponent<Quest>() != null)
            .ToList();

        templateNames = templates.Select(t => t.name).ToArray();
    }

    private void RefreshTypes()
    {
        questTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
            .Where(t => t.IsSubclassOf(typeof(Quest)) && !t.IsAbstract)
            .OrderBy(t => t.Name)
            .ToList();

        questTypeNames = questTypes.Select(t => $"{t.Name} : {t.BaseType?.Name}").ToArray();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Quest Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        questManager = (QuestManager)EditorGUILayout.ObjectField("Quest Manager", questManager, typeof(QuestManager), true);

        EditorGUILayout.Space(4);

        EditorGUILayout.BeginHorizontal();
        selectedTemplate = EditorGUILayout.Popup("Template", selectedTemplate, templateNames.Length > 0 ? templateNames : new[] { "None found" });
        if (GUILayout.Button("↺", GUILayout.Width(26))) RefreshTemplates();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        selectedType = EditorGUILayout.Popup("Quest Type", selectedType, questTypeNames.Length > 0 ? questTypeNames : new[] { "None found" });
        if (GUILayout.Button("↺", GUILayout.Width(26))) RefreshTypes();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        questName = EditorGUILayout.TextField("Name", questName);
        description = EditorGUILayout.TextArea(description, GUILayout.Height(48));
        sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", sprite, typeof(Sprite), false);

        EditorGUILayout.Space(8);

        GUI.enabled = templates.Count > 0 && questManager != null && questTypes.Count > 0;
        if (GUILayout.Button("Create Quest", GUILayout.Height(36))) Create();
        GUI.enabled = true;
    }

    private void Create()
    {
        EnsureFolder(DATA_PATH);
        EnsureFolder(PREFABS_PATH);

        string dataPath = AssetDatabase.GenerateUniqueAssetPath($"{DATA_PATH}/{questName.Replace(" ", "")}Data.asset");
        QuestData data = ScriptableObject.CreateInstance<QuestData>();
        data.questName = questName;
        data.description = description;
        data.sprite = sprite;
        AssetDatabase.CreateAsset(data, dataPath);

        GameObject go = Instantiate(templates[selectedTemplate]);
        go.name = $"{questName.Replace(" ", "")}Quest";
        Quest old = go.GetComponent<Quest>();

        var oldSo = new SerializedObject(old);
        var cachedIcon = oldSo.FindProperty("imageIcon").objectReferenceValue;
        var cachedName = oldSo.FindProperty("NameText").objectReferenceValue;
        var cachedDesc = oldSo.FindProperty("descriptionText").objectReferenceValue;
        var cachedDone = oldSo.FindProperty("CompletedText").objectReferenceValue;
        DestroyImmediate(old);

        Quest newQuest = (Quest)go.AddComponent(questTypes[selectedType]);
        var newSo = new SerializedObject(newQuest);
        newSo.FindProperty("data").objectReferenceValue = data;
        newSo.FindProperty("imageIcon").objectReferenceValue = cachedIcon;
        newSo.FindProperty("NameText").objectReferenceValue = cachedName;
        newSo.FindProperty("descriptionText").objectReferenceValue = cachedDesc;
        newSo.FindProperty("CompletedText").objectReferenceValue = cachedDone;
        newSo.ApplyModifiedPropertiesWithoutUndo();

        string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{PREFABS_PATH}/{go.name}.prefab");
        GameObject saved = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        DestroyImmediate(go);

        var managerSo = new SerializedObject(questManager);
        var list = managerSo.FindProperty("quests");
        list.arraySize++;
        list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = saved.GetComponent<Quest>();
        managerSo.ApplyModifiedProperties();

        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(saved);
    }

    private void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}