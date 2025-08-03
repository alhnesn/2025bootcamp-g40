using UnityEngine;

public class IngredientSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private GameObject ingredientPrefab;
    
    [Header("Debug Info")]
    [SerializeField] private bool showDebugLogs = true;

    /// <summary>
    /// Spawns an ingredient and gives it to the player
    /// </summary>
    /// <param name="playerInteraction">Reference to the player interaction script</param>
    public void SpawnIngredient(PlayerInteraction playerInteraction)
    {
        // Safety checks
        if (ingredientPrefab == null)
        {
            if (showDebugLogs)
                Debug.LogError($"IngredientSpawner '{gameObject.name}' has no ingredient prefab assigned!");
            return;
        }
        
        if (playerInteraction == null)
        {
            if (showDebugLogs)
                Debug.LogError("PlayerInteraction reference is null!");
            return;
        }
        
        if (playerInteraction.IsHoldingItem())
        {
            if (showDebugLogs)
                Debug.Log("Player hands are full, cannot spawn ingredient.");
            return;
        }
        
        // Spawn the ingredient
        GameObject spawnedItem = Instantiate(ingredientPrefab);
        
        if (spawnedItem == null)
        {
            if (showDebugLogs)
                Debug.LogError("Failed to instantiate ingredient prefab!");
            return;
        }
        
        // Verify the spawned item has required components
        if (!ValidateSpawnedItem(spawnedItem))
        {
            Destroy(spawnedItem);
            return;
        }
        
        // Give the spawned item to the player
        playerInteraction.PickupItem(spawnedItem);
        
        if (showDebugLogs)
        {
            string itemName = spawnedItem.GetComponent<Ingredient>()?.ingredientName ?? spawnedItem.name;
            Debug.Log($"Spawned {itemName} from {gameObject.name}");
        }
    }

    /// <summary>
    /// Validates that the spawned item has all required components
    /// </summary>
    /// <param name="item">The spawned item to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    private bool ValidateSpawnedItem(GameObject item)
    {
        // Check for Holdable component (required by PickupItem)
        Holdable holdable = item.GetComponent<Holdable>();
        if (holdable == null)
        {
            if (showDebugLogs)
                Debug.LogError($"Spawned item '{item.name}' is missing Holdable component! Cannot be picked up.");
            return false;
        }
        
        // Check for Ingredient component (good practice)
        Ingredient ingredient = item.GetComponent<Ingredient>();
        if (ingredient == null)
        {
            if (showDebugLogs)
                Debug.LogWarning($"Spawned item '{item.name}' is missing Ingredient component!");
        }
        
        return true;
    }

    /// <summary>
    /// Get the name of the ingredient this spawner creates (for UI/debugging)
    /// </summary>
    /// <returns>The ingredient name or "Unknown" if not set</returns>
    public string GetIngredientName()
    {
        if (ingredientPrefab == null)
            return "None (No Prefab Assigned)";
            
        Ingredient ingredient = ingredientPrefab.GetComponent<Ingredient>();
        return ingredient != null ? ingredient.ingredientName : ingredientPrefab.name;
    }
    
    /// <summary>
    /// Check if this spawner has a valid prefab assigned
    /// </summary>
    /// <returns>True if prefab is assigned and valid</returns>
    public bool HasValidPrefab()
    {
        return ingredientPrefab != null && ingredientPrefab.GetComponent<Holdable>() != null;
    }
    
    // Editor helper - shows spawner info in inspector
    void OnValidate()
    {
        if (ingredientPrefab != null)
        {
            // Check if prefab has required components
            Holdable holdable = ingredientPrefab.GetComponent<Holdable>();
            if (holdable == null)
            {
                Debug.LogWarning($"Assigned prefab '{ingredientPrefab.name}' in IngredientSpawner '{gameObject.name}' is missing Holdable component!");
            }
        }
    }
}
