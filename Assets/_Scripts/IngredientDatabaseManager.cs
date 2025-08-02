using UnityEngine;

public class IngredientDatabaseManager : MonoBehaviour
{
    [Header("Database Reference")]
    public IngredientDatabase database;
    
    private static IngredientDatabaseManager instance;
    public static IngredientDatabaseManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<IngredientDatabaseManager>();
                if (instance == null)
                {
                    Debug.LogError("IngredientDatabaseManager not found in scene!");
                }
            }
            return instance;
        }
    }
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public static float GetIngredientPrice(string ingredientName)
    {
        return Instance?.database?.GetPrice(ingredientName) ?? 1.0f;
    }
    
    public static float GetIngredientTime(string ingredientName)
    {
        return Instance?.database?.GetPreparationTime(ingredientName) ?? 5.0f;
    }
    
    public static bool HasIngredient(string ingredientName)
    {
        return Instance?.database?.HasIngredient(ingredientName) ?? false;
    }
}
