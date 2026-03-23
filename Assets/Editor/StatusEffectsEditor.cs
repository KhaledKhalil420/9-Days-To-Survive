using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;

public class StatusEffectEditor : EditorWindow
{
    string effectName;
    Sprite effectSprite;
    int selectedTypeIndex;
    System.Type[] effectTypes;
    string[] effectTypeNames;

    Dictionary<FieldInfo, object> editableFields = new Dictionary<FieldInfo, object>();
    Vector2 scrollPos;

    StatusEffectData createdData;
    GameObject createdPrefab;

    [MenuItem("Khaled/Status Effect Editor")]
    public static void ShowWindow()
    {
        GetWindow<StatusEffectEditor>("Status Effect Editor");
    }

    void OnEnable()
    {
        effectTypes = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(StatusEffect).IsAssignableFrom(t) && !t.IsAbstract)
            .ToArray();

        effectTypeNames = effectTypes.Select(t => t.Name).ToArray();
        LoadEditableFields();
    }

    void LoadEditableFields()
    {
        editableFields.Clear();
        if (effectTypes == null || effectTypes.Length == 0) return;

        System.Type selectedType = effectTypes[selectedTypeIndex];

        // Spin up a temp object to pull real default values from
        GameObject temp = new GameObject("__temp__");
        temp.hideFlags = HideFlags.HideAndDontSave;
        StatusEffect tempEffect = (StatusEffect)temp.AddComponent(selectedType);

        var fields = selectedType
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f =>
                f.Name != "data" &&
                f.Name != "target" &&
                !f.GetCustomAttributes().Any(a => a.GetType().Name == "ReadOnlyAttribute"));

        foreach (var field in fields)
            editableFields[field] = field.GetValue(tempEffect);

        DestroyImmediate(temp);
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("Status Effect Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        effectName = EditorGUILayout.TextField("Effect Name", effectName);
        effectSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", effectSprite, typeof(Sprite), false);

        EditorGUI.BeginChangeCheck();
        selectedTypeIndex = EditorGUILayout.Popup("Effect Type", selectedTypeIndex, effectTypeNames);
        if (EditorGUI.EndChangeCheck())
            LoadEditableFields();

        // Editable fields from type
        if (editableFields.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Effect Properties:", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            foreach (var kvp in editableFields.ToList())
            {
                var field = kvp.Key;
                string label = PrettifyFieldName(field.Name);

                if (field.FieldType == typeof(int))
                    editableFields[field] = EditorGUILayout.IntField(label, (int)kvp.Value);
                else if (field.FieldType == typeof(float))
                    editableFields[field] = EditorGUILayout.FloatField(label, (float)kvp.Value);
                else if (field.FieldType == typeof(bool))
                    editableFields[field] = EditorGUILayout.Toggle(label, (bool)kvp.Value);
                else if (field.FieldType == typeof(string))
                    editableFields[field] = EditorGUILayout.TextField(label, (string)kvp.Value);
                else if (field.FieldType.IsEnum)
                    editableFields[field] = EditorGUILayout.EnumPopup(label, (System.Enum)kvp.Value);
                else if (typeof(Object).IsAssignableFrom(field.FieldType))
                    editableFields[field] = EditorGUILayout.ObjectField(label, (Object)kvp.Value, field.FieldType, false);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        if (GUILayout.Button("Create Status Effect", GUILayout.Height(30)))
            CreateStatusEffect();

        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Created Data", createdData, typeof(StatusEffectData), false);
        EditorGUILayout.ObjectField("Created Prefab", createdPrefab, typeof(GameObject), false);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndScrollView();
    }

    void CreateStatusEffect()
    {
        StatusEffectData data = ScriptableObject.CreateInstance<StatusEffectData>();
        data.statusName = effectName;
        data.sprite = effectSprite;

        System.Type selectedType = effectTypes[selectedTypeIndex];
        GameObject obj = new GameObject(effectName);
        obj.AddComponent(selectedType);

        StatusEffect effect = obj.GetComponent<StatusEffect>();
        effect.data = data;

        foreach (var kvp in editableFields)
            kvp.Key.SetValue(effect, kvp.Value);

        AssetDatabase.CreateAsset(data, $"Assets/Resources/StatusEffects/{effectName}Data.asset");
        AssetDatabase.SaveAssets();

        createdPrefab = PrefabUtility.SaveAsPrefabAsset(obj, $"Assets/Prefabs/StatusEffects/{effectName}.prefab");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        DestroyImmediate(obj);

        createdData = Resources.Load<StatusEffectData>($"StatusEffects/{effectName}Data");

        EditorUtility.DisplayDialog("Success", $"'{effectName}' created!", "OK");

        effectName = "";
        effectSprite = null;
        LoadEditableFields();
    }

    string PrettifyFieldName(string fieldName)
    {
        string result = Regex.Replace(fieldName, "([a-z])([A-Z])", "$1 $2");
        result = Regex.Replace(result, "([a-zA-Z])([0-9])", "$1 $2");
        if (result.Length > 0)
            result = char.ToUpper(result[0]) + result.Substring(1);
        return result;
    }
}