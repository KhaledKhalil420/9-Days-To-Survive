using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;

public class ItemEditor : EditorWindow
{
    string itemName;
    string itemDescription;
    Sprite itemSprite;
    GameObject sourceModel;
    GameObject itemTemplate;
    GameObject lastTemplate;
    
    // Template categories
    string[] templateCategories;
    string[] templatePaths;
    int selectedCategoryIndex = 0;
    
    // Recipe settings
    bool createRecipe = false;
    int givenQuantity = 1;
    List<RecipeIngredient> recipeIngredients = new List<RecipeIngredient>();
    Vector2 scrollPos;
    
    // EditorChangeable fields
    Dictionary<FieldInfo, object> editableFields = new Dictionary<FieldInfo, object>();
    bool showEditableFields = false;
    System.Type currentItemType;
    
    [System.Serializable]
    class RecipeIngredient
    {
        public ItemData item;
        public int quantity = 1;
    }
    
    [MenuItem("Khaled/Item Editor")]
    public static void ShowWindow()
    {
        GetWindow<ItemEditor>("Item Editor");
    }

    void OnEnable()
    {
        LoadTemplateCategories();
        LoadEditableFields();
    }

    void LoadTemplateCategories()
    {
        string templatesPath = "Assets/Prefabs/Items/Templates";
        
        if (!Directory.Exists(templatesPath))
        {
            templateCategories = new string[0];
            templatePaths = new string[0];
            return;
        }

        var templateFiles = Directory.GetFiles(templatesPath, "*.prefab", SearchOption.TopDirectoryOnly);
        
        templateCategories = new string[templateFiles.Length];
        templatePaths = new string[templateFiles.Length];
        
        for (int i = 0; i < templateFiles.Length; i++)
        {
            templatePaths[i] = templateFiles[i];
            templateCategories[i] = Path.GetFileNameWithoutExtension(templateFiles[i]);
        }
    }

    void LoadEditableFields()
    {
        editableFields.Clear();
        currentItemType = null;
        
        if (itemTemplate == null)
        {
            showEditableFields = false;
            return;
        }
        
        Item itemComponent = itemTemplate.GetComponent<Item>();
        if (itemComponent == null)
        {
            showEditableFields = false;
            return;
        }
        
        currentItemType = itemComponent.GetType();
        
        var fields = currentItemType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetCustomAttribute<EditorChangeableAttribute>() != null);
        
        foreach (var field in fields)
        {
            object defaultValue = field.GetValue(itemComponent);
            editableFields[field] = defaultValue;
        }
        
        showEditableFields = editableFields.Count > 0;
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        GUILayout.Label("Item Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        itemName = EditorGUILayout.TextField("Item Name", itemName);
        itemDescription = EditorGUILayout.TextField("Description", itemDescription);
        itemSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", itemSprite, typeof(Sprite), false);
        sourceModel = (GameObject)EditorGUILayout.ObjectField("Source Model", sourceModel, typeof(GameObject), false);
        
        // Template category dropdown
        if (templateCategories.Length > 0)
        {
            EditorGUI.BeginChangeCheck();
            selectedCategoryIndex = EditorGUILayout.Popup("Template Category", selectedCategoryIndex, templateCategories);
            if (EditorGUI.EndChangeCheck())
            {
                itemTemplate = AssetDatabase.LoadAssetAtPath<GameObject>(templatePaths[selectedCategoryIndex]);
                lastTemplate = itemTemplate;
                LoadEditableFields();
            }
        }
        
        EditorGUI.BeginChangeCheck();
        itemTemplate = (GameObject)EditorGUILayout.ObjectField("Template Prefab", itemTemplate, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck() || itemTemplate != lastTemplate)
        {
            lastTemplate = itemTemplate;
            LoadEditableFields();
        }

        // EditorChangeable fields section
        if (showEditableFields)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Item Properties:", EditorStyles.boldLabel);
            
            foreach (var kvp in editableFields.ToList())
            {
                var field = kvp.Key;
                var attribute = field.GetCustomAttribute<EditorChangeableAttribute>();
                string displayName = attribute.DisplayName ?? field.Name;
                
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
                    editableFields[field] = EditorGUILayout.TextField(displayName, (string)kvp.Value);
                }
                else if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
                {
                    editableFields[field] = EditorGUILayout.ObjectField(displayName, (UnityEngine.Object)kvp.Value, field.FieldType, false);
                }
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        //Recipe section
        createRecipe = EditorGUILayout.Toggle("Create Crafting Recipe", createRecipe);
        
        if (createRecipe)
        {
            EditorGUI.indentLevel++;
            givenQuantity = EditorGUILayout.IntField("Given Quantity", givenQuantity);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ingredients:", EditorStyles.boldLabel);
            
            for (int i = 0; i < recipeIngredients.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                recipeIngredients[i].item = (ItemData)EditorGUILayout.ObjectField(recipeIngredients[i].item, typeof(ItemData), false);
                recipeIngredients[i].quantity = EditorGUILayout.IntField(recipeIngredients[i].quantity, GUILayout.Width(50));
                
                if (GUILayout.Button("X", GUILayout.Width(30)))
                {
                    recipeIngredients.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Ingredient"))
            {
                recipeIngredients.Add(new RecipeIngredient());
            }
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Item", GUILayout.Height(30)))
        {
            CreateItem();
        }
        
        EditorGUILayout.EndScrollView();
    }

    void CreateItem()
    {
        if (string.IsNullOrEmpty(itemName))
        {
            EditorUtility.DisplayDialog("Error", "Item name cannot be empty!", "OK");
            return;
        }
    
        if (itemTemplate == null)
        {
            EditorUtility.DisplayDialog("Error", "Template prefab is required!", "OK");
            return;
        }
    
        //Create ItemData
        ItemData data = ScriptableObject.CreateInstance<ItemData>();
        data.Name = itemName;
        data.discription = itemDescription;
        data.sprite = itemSprite;
    
        //Instantiate from template (CLONE)
        GameObject clone = Instantiate(itemTemplate);
        clone.name = itemName;
    
        //Copy visuals from source model
        if (sourceModel != null)
        {
            //CLEAR ALL EXISTING VISUAL CHILDREN FROM CLONE FIRST
            List<Transform> childrenToDelete = new List<Transform>();
            foreach (Transform child in clone.transform)
            {
                childrenToDelete.Add(child);
            }
            foreach (Transform child in childrenToDelete)
            {
                DestroyImmediate(child.gameObject);
            }
    
            //Get source mesh and materials from SOURCE ROOT
            MeshFilter sourceMeshFilter = sourceModel.GetComponent<MeshFilter>();
            MeshRenderer sourceMeshRenderer = sourceModel.GetComponent<MeshRenderer>();
    
            //Apply mesh to CLONE ITSELF
            if (sourceMeshFilter != null)
            {
                MeshFilter cloneMeshFilter = clone.GetComponent<MeshFilter>();
                if (cloneMeshFilter == null)
                    cloneMeshFilter = clone.AddComponent<MeshFilter>();
                
                cloneMeshFilter.sharedMesh = sourceMeshFilter.sharedMesh;
            }
    
            //Apply materials to CLONE ITSELF
            if (sourceMeshRenderer != null)
            {
                MeshRenderer cloneMeshRenderer = clone.GetComponent<MeshRenderer>();
                if (cloneMeshRenderer == null)
                    cloneMeshRenderer = clone.AddComponent<MeshRenderer>();
                
                cloneMeshRenderer.sharedMaterials = sourceMeshRenderer.sharedMaterials;
            }
    
            //NOW copy ONLY the children from source to clone
            foreach (Transform sourceChild in sourceModel.transform)
            {
                CopyChildRecursive(sourceChild, clone.transform);
            }
        }
    
        // Reset collider to fit the new mesh
        Collider collider = clone.GetComponent<Collider>();
        if (collider != null)
        {
            System.Type colliderType = collider.GetType();
            DestroyImmediate(collider);
            
            if (colliderType == typeof(BoxCollider))
            {
                clone.AddComponent<BoxCollider>();
            }
            else if (colliderType == typeof(SphereCollider))
            {
                clone.AddComponent<SphereCollider>();
            }
            else if (colliderType == typeof(CapsuleCollider))
            {
                clone.AddComponent<CapsuleCollider>();
            }
            else if (colliderType == typeof(MeshCollider))
            {
                MeshCollider meshCollider = clone.AddComponent<MeshCollider>();
                meshCollider.convex = true;
            }
        }
        else
        {
            MeshCollider meshCollider = clone.AddComponent<MeshCollider>();
            meshCollider.convex = true;
        }
    
        Item itemComponent = clone.GetComponent<Item>();
        itemComponent.data = data;
        
        foreach (var kvp in editableFields)
        {
            kvp.Key.SetValue(itemComponent, kvp.Value);
        }
    
        string dataPath = "Assets/Resources/ItemData";
        AssetDatabase.CreateAsset(data, $"{dataPath}/{itemName}Data.asset");
    
        string prefabPath = "Assets/Prefabs/Items";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(clone, $"{prefabPath}/{itemName}.prefab");
        
        data.prefab = prefab;
        
        if (createRecipe && recipeIngredients.Count > 0)
        {
            CreateCraftingRecipe(itemComponent);
        }
        
        AssetDatabase.SaveAssets();
        DestroyImmediate(clone);
        
        EditorUtility.DisplayDialog("Success", $"Item '{itemName}' created successfully!" + (createRecipe ? "\nRecipe created!" : ""), "OK");
        
        itemName = "";
        itemDescription = "";
        itemSprite = null;
        sourceModel = null;
        createRecipe = false;
        givenQuantity = 1;
        recipeIngredients.Clear();
        LoadEditableFields();
    }

    void CopyChildRecursive(Transform sourceChild, Transform targetParent)
    {
        GameObject newChild = new GameObject(sourceChild.name);
        newChild.transform.SetParent(targetParent);
        newChild.transform.localPosition = sourceChild.localPosition;
        newChild.transform.localRotation = sourceChild.localRotation;
        newChild.transform.localScale = sourceChild.localScale;

        MeshFilter sourceMeshFilter = sourceChild.GetComponent<MeshFilter>();
        if (sourceMeshFilter != null)
        {
            MeshFilter newMeshFilter = newChild.AddComponent<MeshFilter>();
            newMeshFilter.sharedMesh = sourceMeshFilter.sharedMesh;
        }

        MeshRenderer sourceMeshRenderer = sourceChild.GetComponent<MeshRenderer>();
        if (sourceMeshRenderer != null)
        {
            MeshRenderer newMeshRenderer = newChild.AddComponent<MeshRenderer>();
            newMeshRenderer.sharedMaterials = sourceMeshRenderer.sharedMaterials;
        }

        //Recursively copy this child's children
        foreach (Transform grandChild in sourceChild)
        {
            CopyChildRecursive(grandChild, newChild.transform);
        }
    }
    
    void CreateCraftingRecipe(Item itemComponent)
    {
        CraftingRecipe recipe = ScriptableObject.CreateInstance<CraftingRecipe>();
        recipe.itemToGive = itemComponent;
        recipe.givenQuantity = givenQuantity;
        recipe.ingredients = new List<Ingredient>();
        
        foreach (var recipeIngredient in recipeIngredients)
        {
            if (recipeIngredient.item != null && recipeIngredient.item.prefab != null)
            {
                Ingredient ingredient = new Ingredient();
                ingredient.item = recipeIngredient.item.prefab.GetComponent<Item>();
                ingredient.quantity = recipeIngredient.quantity;
                recipe.ingredients.Add(ingredient);
            }
        }
        
        string recipePath = "Assets/Resources/RecipeData";
        AssetDatabase.CreateAsset(recipe, $"{recipePath}/{itemName}Recipe.asset");
    }
}