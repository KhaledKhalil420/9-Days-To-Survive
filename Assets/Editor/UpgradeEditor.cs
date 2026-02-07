using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;

public class UpgradeEditor : EditorWindow
{
    string upgName;
    int upgPrice;
    Sprite upgSprite;
    int selectedUpgradeIndex;
    System.Type[] upgradeTypes;
    string[] upgradeNames;

    [MenuItem("Window/Upgrade Editor")]
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
        upgSprite = (Sprite)EditorGUILayout.ObjectField("Enter upgrade sprite", upgSprite, typeof(Sprite), false);
        selectedUpgradeIndex = EditorGUILayout.Popup("Select upgrade script", selectedUpgradeIndex, upgradeNames);

        if(GUILayout.Button("Create new upgrade data"))
        {
            //Creating Data
            UpgradeData data = new();
            data.fullName = upgName;
            data.price = upgPrice;
            data.sprite = upgSprite;

            
            //Creating Object
            System.Type selectedType = upgradeTypes[selectedUpgradeIndex];
            GameObject upgradeObject = new(upgName);
            upgradeObject.AddComponent(selectedType);
            Upgrade ugpgrade = upgradeObject.GetComponent<Upgrade>();
            ugpgrade.data = data;

            //Saving
            AssetDatabase.CreateAsset(data, $"Assets/Resources/Upgrades/{upgName}Data.asset");
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(upgradeObject, $"Assets/Prefabs/Upgrades/{upgName}.prefab");

            data.upgrade = prefab;
            AssetDatabase.SaveAssets();
            DestroyImmediate(upgradeObject);
        }
    }
}