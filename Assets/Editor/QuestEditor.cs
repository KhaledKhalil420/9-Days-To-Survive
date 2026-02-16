using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class QuestEditor : EditorWindow
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
    private int lastSelectedType = -1;

    private string questName = "New Quest";
    private string description = "Description...";
    private Sprite sprite;

    private QuestManager questManager;
    
    // Editable fields
    private Dictionary<FieldInfo, object> editableFields = new Dictionary<FieldInfo, object>();
    private Dictionary<string, bool> categoryFoldouts = new Dictionary<string, bool>();
    private bool showEditableFields = false;
    private Vector2 scrollPos;

    [MenuItem("Khaled/Quest Creator")]
    public static void Open()
    {
        var w = GetWindow<QuestEditor>("Quest Creator");
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
    
    private void LoadEditableFields()
    {
        editableFields.Clear();
        categoryFoldouts.Clear();
        
        if (selectedType < 0 || selectedType >= questTypes.Count)
        {
            showEditableFields = false;
            return;
        }
        
        Type questType = questTypes[selectedType];
        
        // Get all fields that Unity would serialize automatically
        var fields = questType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => IsSerializableField(f) && !IsBaseQuestField(f));
        
        foreach (var field in fields)
        {
            object defaultValue = GetDefaultValue(field.FieldType);
            editableFields[field] = defaultValue;
            
            string category = GetAutoCategory(field.Name);
            if (!categoryFoldouts.ContainsKey(category))
            {
                categoryFoldouts[category] = true;
            }
        }
        
        showEditableFields = editableFields.Count > 0;
    }
    
    private bool IsSerializableField(FieldInfo field)
    {
        // Skip if marked with NonSerialized or HideInInspector
        if (field.GetCustomAttribute<NonSerializedAttribute>() != null ||
            field.GetCustomAttribute<HideInInspector>() != null)
            return false;
        
        // Include if public OR has SerializeField attribute
        bool isPublic = field.IsPublic;
        bool hasSerializeField = field.GetCustomAttribute<SerializeField>() != null;
        
        if (!isPublic && !hasSerializeField)
            return false;
        
        // Check if type is serializable by Unity
        Type fieldType = field.FieldType;
        
        return fieldType.IsPrimitive ||
               fieldType == typeof(string) ||
               fieldType.IsEnum ||
               typeof(UnityEngine.Object).IsAssignableFrom(fieldType) ||
               fieldType == typeof(Vector2) ||
               fieldType == typeof(Vector3) ||
               fieldType == typeof(Vector4) ||
               fieldType == typeof(Color) ||
               fieldType == typeof(Rect) ||
               fieldType == typeof(AnimationCurve);
    }
    
    private bool IsBaseQuestField(FieldInfo field)
    {
        // Exclude fields that are part of the base Quest class
        return field.DeclaringType == typeof(Quest) || 
               field.Name == "data" || 
               field.Name == "imageIcon" || 
               field.Name == "NameText" || 
               field.Name == "descriptionText" || 
               field.Name == "CompletedText";
    }
    
    private object GetDefaultValue(Type type)
    {
        if (type.IsValueType)
            return Activator.CreateInstance(type);
        return null;
    }

    private string GetAutoCategory(string fieldName)
    {
        string baseName = Regex.Replace(fieldName, @"\d+", "").ToLower();
        
        if (baseName.Contains("target") || baseName.Contains("goal") || baseName.Contains("required"))
            return "Quest Objectives";
        if (baseName.Contains("reward") || baseName.Contains("xp") || baseName.Contains("gold"))
            return "Rewards";
        if (baseName.Contains("item") || baseName.Contains("collect"))
            return "Items";
        if (baseName.Contains("enemy") || baseName.Contains("kill") || baseName.Contains("defeat"))
            return "Combat";
        if (baseName.Contains("npc") || baseName.Contains("talk") || baseName.Contains("dialogue"))
            return "NPCs";
        if (baseName.Contains("time") || baseName.Contains("duration") || baseName.Contains("delay"))
            return "Timing";
        if (baseName.Contains("location") || baseName.Contains("position") || baseName.Contains("area"))
            return "Location";
        
        return "Quest Settings";
    }

    private string PrettifyFieldName(string fieldName)
    {
        string result = Regex.Replace(fieldName, "([a-z])([A-Z])", "$1 $2");
        result = Regex.Replace(result, "([a-zA-Z])([0-9])", "$1 $2");
        if (result.Length > 0)
            result = char.ToUpper(result[0]) + result.Substring(1);
        return result;
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
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
        EditorGUI.BeginChangeCheck();
        selectedType = EditorGUILayout.Popup("Quest Type", selectedType, questTypeNames.Length > 0 ? questTypeNames : new[] { "None found" });
        if (EditorGUI.EndChangeCheck() && selectedType != lastSelectedType)
        {
            lastSelectedType = selectedType;
            LoadEditableFields();
        }
        if (GUILayout.Button("↺", GUILayout.Width(26)))
        {
            RefreshTypes();
            LoadEditableFields();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        questName = EditorGUILayout.TextField("Name", questName);
        description = EditorGUILayout.TextArea(description, GUILayout.Height(48));
        sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", sprite, typeof(Sprite), false);

        // Show editable fields
        if (showEditableFields)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quest Properties:", EditorStyles.boldLabel);
            
            var groupedFields = editableFields
                .GroupBy(kvp => GetAutoCategory(kvp.Key.Name))
                .OrderBy(g => g.Key);
            
            foreach (var group in groupedFields)
            {
                string category = group.Key;
                
                categoryFoldouts[category] = EditorGUILayout.Foldout(categoryFoldouts[category], category, true, EditorStyles.foldoutHeader);
                
                if (categoryFoldouts[category])
                {
                    EditorGUI.indentLevel++;
                    
                    foreach (var kvp in group.OrderBy(x => x.Key.Name))
                    {
                        var field = kvp.Key;
                        string displayName = PrettifyFieldName(field.Name);
                        
                        if (field.FieldType == typeof(int))
                        {
                            editableFields[field] = EditorGUILayout.IntField(displayName, (int)kvp.Value);
                        }
                        else if (field.FieldType == typeof(float))
                        {
                            editableFields[field] = EditorGUILayout.FloatField(displayName, (float)kvp.Value);
                        }
                        else if (field.FieldType == typeof(bool))
                        {
                            editableFields[field] = EditorGUILayout.Toggle(displayName, (bool)kvp.Value);
                        }
                        else if (field.FieldType == typeof(string))
                        {
                            editableFields[field] = EditorGUILayout.TextField(displayName, (string)kvp.Value ?? "");
                        }
                        else if (field.FieldType.IsEnum)
                        {
                            editableFields[field] = EditorGUILayout.EnumPopup(displayName, (System.Enum)kvp.Value);
                        }
                        else if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
                        {
                            editableFields[field] = EditorGUILayout.ObjectField(displayName, (UnityEngine.Object)kvp.Value, field.FieldType, false);
                        }
                        else if (field.FieldType == typeof(Vector2))
                        {
                            editableFields[field] = EditorGUILayout.Vector2Field(displayName, (Vector2)kvp.Value);
                        }
                        else if (field.FieldType == typeof(Vector3))
                        {
                            editableFields[field] = EditorGUILayout.Vector3Field(displayName, (Vector3)kvp.Value);
                        }
                        else if (field.FieldType == typeof(Color))
                        {
                            editableFields[field] = EditorGUILayout.ColorField(displayName, (Color)kvp.Value);
                        }
                    }
                    
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.Space();
            }
        }

        EditorGUILayout.Space(8);

        GUI.enabled = templates.Count > 0 && questManager != null && questTypes.Count > 0;
        if (GUILayout.Button("Create Quest", GUILayout.Height(36))) Create();
        GUI.enabled = true;
        
        EditorGUILayout.EndScrollView();
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
    
    // Apply editable fields using SerializedObject
    newSo.Update(); // Make sure we have the latest data
    foreach (var kvp in editableFields)
    {
        SerializedProperty prop = newSo.FindProperty(kvp.Key.Name);
        if (prop != null)
        {
            SetSerializedPropertyValue(prop, kvp.Value);
        }
    }
    newSo.ApplyModifiedPropertiesWithoutUndo(); // Apply again after setting custom fields

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

private void SetSerializedPropertyValue(SerializedProperty prop, object value)
{
    if (value == null) return;
    
    switch (prop.propertyType)
    {
        case SerializedPropertyType.Integer:
            prop.intValue = (int)value;
            break;
        case SerializedPropertyType.Boolean:
            prop.boolValue = (bool)value;
            break;
        case SerializedPropertyType.Float:
            prop.floatValue = (float)value;
            break;
        case SerializedPropertyType.String:
            prop.stringValue = (string)value;
            break;
        case SerializedPropertyType.ObjectReference:
            prop.objectReferenceValue = (UnityEngine.Object)value;
            break;
        case SerializedPropertyType.Enum:
            prop.enumValueIndex = (int)value;
            break;
        case SerializedPropertyType.Vector2:
            prop.vector2Value = (Vector2)value;
            break;
        case SerializedPropertyType.Vector3:
            prop.vector3Value = (Vector3)value;
            break;
        case SerializedPropertyType.Color:
            prop.colorValue = (Color)value;
            break;
    }
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