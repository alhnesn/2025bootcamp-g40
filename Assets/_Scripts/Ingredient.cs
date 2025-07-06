using UnityEngine;

// We define the possible states for an ingredient outside the class
// so other scripts can easily reference them.
public enum IngredientState { Whole, Chopped, Cooked }

public class Ingredient : MonoBehaviour
{
    public string ingredientName;
    public IngredientState currentState;

    // This will hold the "Chopped" version of this ingredient.
    public GameObject processedPrefab;
}