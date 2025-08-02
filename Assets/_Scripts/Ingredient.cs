using UnityEngine;

// We define the possible states for an ingredient outside the class
// so other scripts can easily reference them.
public enum IngredientState { Whole, Chopped, Cooked }

public class Ingredient : MonoBehaviour
{
    public string ingredientName;
    public IngredientState currentState;

    [Header("Order System")]
    public float price = 1.0f;           // Price for this ingredient
    public float preparationTime = 5.0f; // Time needed to prepare this ingredient
    
}