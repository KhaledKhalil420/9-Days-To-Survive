using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;

public class UpgradeEditor : EditorWindow
{
    string upgName;
    int upgPrice;
    string upgDiscreption;
    Sprite upgSprite;
    int selectedUpgradeIndex;
    System.Type[] upgradeTypes;
    string[] upgradeNames;
    GameObject prefab;
    UpgradeData upgradeData;

    [MenuItem("Khaled/Upgrade Editor")]
    public static void ShowWindow()
    {
        GetWindow<UpgradeEditor>("Upgrade Editor");
    }

    void OnEnable()
    {
        upgradeTypes = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(t => typeof(Upgrade).IsAssignableFrom(t) && !t.IsAbstract).ToArray();
        upgradeNames = upgradeTypes.Select(t => t.Name).ToArray();
    }

    void OnGUI()
    {
        upgName = EditorGUILayout.TextField("Enter upgrade name", upgName);
        upgPrice = EditorGUILayout.IntField("Enter upgrade price", upgPrice);
        upgDiscreption = EditorGUILayout.TextField("Enter upgrade discreption", upgDiscreption);
        upgSprite = (Sprite)EditorGUILayout.ObjectField("Enter upgrade sprite", upgSprite, typeof(Sprite), false);
        selectedUpgradeIndex = EditorGUILayout.Popup("Select upgrade script", selectedUpgradeIndex, upgradeNames);

        UpgradeData data = new();
        
        if(GUILayout.Button("Create new upgrade data"))
        {
            //Creating Data
            data.fullName = upgName;
            data.price = upgPrice;
            data.discription = upgDiscreption;
            data.sprite = upgSprite;
            
            //Creating Object
            System.Type selectedType = upgradeTypes[selectedUpgradeIndex];
            GameObject upgradeObject = new(upgName);
            upgradeObject.AddComponent(selectedType);
            Upgrade ugpgrade = upgradeObject.GetComponent<Upgrade>();
            ugpgrade.data = data;

            //Saving
            AssetDatabase.CreateAsset(data, $"Assets/Resources/Upgrades/{upgName}Data.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            upgradeData = Resources.Load<UpgradeData>($"Upgrades/{upgName}Data");

            prefab = PrefabUtility.SaveAsPrefabAsset(upgradeObject, $"Assets/Prefabs/Upgrades/{upgName}.prefab");

            data.upgrade = prefab;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            DestroyImmediate(upgradeObject);
            DestroyImmediate(upgradeObject);

        }

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Upgrade Data Reference", upgradeData, typeof(UpgradeData), false);
        EditorGUILayout.ObjectField("Upgrade Object Reference", prefab, typeof(GameObject), false);
        EditorGUI.EndDisabledGroup();
    }
}