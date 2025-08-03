using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IngredientDatabase", menuName = "Game/Ingredient Database")]
public class IngredientDatabase : ScriptableObject
{
    [Header("All Available Ingredients")]
    public List<GameObject> ingredientPrefabs = new List<GameObject>();
    
    // Cache for fast lookup
    private Dictionary<string, IngredientData> ingredientLookup;
    
    [System.Serializable]
    public class IngredientData
    {
        public GameObject prefab;
        public Ingredient component;
        public string name;
        public float price;
        public float preparationTime;
        
        public IngredientData(GameObject prefab, Ingredient component)
        {
            this.prefab = prefab;
            this.component = component;
            this.name = component.ingredientName;
            this.price = component.price;
            this.preparationTime = component.preparationTime;
        }
    }

    void OnEnable()
    {
        BuildLookupCache();
    }

    private void BuildLookupCache()
    {
        ingredientLookup = new Dictionary<string, IngredientData>();
        
        foreach (GameObject prefab in ingredientPrefabs)
        {
            if (prefab == null) continue;
            
            Ingredient ingredient = prefab.GetComponent<Ingredient>();
            if (ingredient == null)
            {
                Debug.LogWarning($"Prefab {prefab.name} has no Ingredient component!");
                continue;
            }
            
            string ingredientName = ingredient.ingredientName;
            if (string.IsNullOrEmpty(ingredientName))
            {
                Debug.LogWarning($"Ingredient on {prefab.name} has empty ingredientName!");
                continue;
            }
            
            if (ingredientLookup.ContainsKey(ingredientName))
            {
                Debug.LogWarning($"Duplicate ingredient name: {ingredientName}");
                continue;
            }
            
            ingredientLookup[ingredientName] = new IngredientData(prefab, ingredient);
        }
        
        Debug.Log($"Ingredient database loaded with {ingredientLookup.Count} ingredients");
    }

    public bool HasIngredient(string ingredientName)
    {
        if (ingredientLookup == null) BuildLookupCache();
        return ingredientLookup.ContainsKey(ingredientName);
    }
    
    public float GetPrice(string ingredientName)
    {
        if (ingredientLookup == null) BuildLookupCache();
        
        if (ingredientLookup.TryGetValue(ingredientName, out IngredientData data))
        {
            return data.price;
        }
        
        Debug.LogWarning($"Ingredient '{ingredientName}' not found in database! Using default price.");
        return 1.0f; // Default price
    }

    public float GetPreparationTime(string ingredientName)
    {
        if (ingredientLookup == null) BuildLookupCache();
        
        if (ingredientLookup.TryGetValue(ingredientName, out IngredientData data))
        {
            return data.preparationTime;
        }
        
        Debug.LogWarning($"Ingredient '{ingredientName}' not found in database! Using default time.");
        return 5.0f; // Default time
    }
    
    public GameObject GetPrefab(string ingredientName)
    {
        if (ingredientLookup == null) BuildLookupCache();
        
        if (ingredientLookup.TryGetValue(ingredientName, out IngredientData data))
        {
            return data.prefab;
        }
        
        return null;
    }

    public List<string> GetAllIngredientNames()
    {
        if (ingredientLookup == null) BuildLookupCache();
        return new List<string>(ingredientLookup.Keys);
    }

    public Sprite GetThumbnail(string ingredientName)
    {
        if (ingredientLookup == null) BuildLookupCache();
        
        if (ingredientLookup.TryGetValue(ingredientName, out IngredientData data))
        {
            return data.component.thumbnailSprite;
        }
        
        return null;
    }
    
    // Validation method for editor
    [ContextMenu("Validate Database")]
    public void ValidateDatabase()
    {
        BuildLookupCache();
        
        foreach (var kvp in ingredientLookup)
        {
            Debug.Log($"✓ {kvp.Key}: ${kvp.Value.price:F2}, {kvp.Value.preparationTime}s");
        }
    }
}
