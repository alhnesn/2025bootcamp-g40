// HamburgerOrder.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HamburgerOrder : Order
{
    [Header("Hamburger Generation Settings")]
    public int minBurgers = 1;
    public int maxBurgers = 3;
    public int minExtraPerSpace = 0;
    public int maxExtraPerSpace = 2;
    
    [Header("Hamburger Order")]
    public List<string> requiredIngredients = new List<string>(); // Bottom to top
    
    // Extra ingredients that can be added between burgers/buns
    private static readonly string[] extraIngredients = {
        "LettuceSliced",
        "TomatoSliced", 
        "OnionSliced",
        "CheeseSliced"
    };
    
    public HamburgerOrder()
    {
        orderType = OrderType.Hamburger;
    }
    
    public override void GenerateRandomOrder()
    {
        requiredIngredients.Clear();
        
        // Step 1: Add bottom bun
        requiredIngredients.Add("BunBottom");
        
        // Step 2: Determine number of burgers
        int burgerCount = Random.Range(minBurgers, maxBurgers + 1);
        Debug.Log($"Generating order with {burgerCount} burgers");
        
        // Step 3: Create spaces and add ingredients
        // Number of spaces = burgerCount + 1 (before first burger, between burgers, after last burger)
        int spaceCount = burgerCount + 1;
        
        for (int space = 0; space < spaceCount; space++)
        {
            // Add extra ingredients to this space
            AddExtrasToSpace(space);
            
            // Add burger after this space (except for the last space)
            if (space < spaceCount - 1)
            {
                requiredIngredients.Add("BurgerCooked");
            }
        }
        
        // Step 4: Add top bun
        requiredIngredients.Add("BunTop");
        
        // Calculate time and price
        timeLimit = CalculateTotalTime();
        totalPrice = CalculateTotalPrice();
        
        Debug.Log($"Generated hamburger order: {string.Join(" → ", requiredIngredients)}");
        Debug.Log($"Time limit: {timeLimit}s, Total price: ${totalPrice:F2}");
    }

    private void AddExtrasToSpace(int spaceIndex)
    {
        // Determine how many extra ingredients for this space
        int extraCount = Random.Range(minExtraPerSpace, maxExtraPerSpace + 1);
        
        if (extraCount == 0) return;
        
        Debug.Log($"Adding {extraCount} extra ingredients to space {spaceIndex}");
        
        // Randomly select extra ingredients (with duplicates allowed)
        for (int i = 0; i < extraCount; i++)
        {
            string randomExtra = extraIngredients[Random.Range(0, extraIngredients.Length)];
            requiredIngredients.Add(randomExtra);
            Debug.Log($"  Added {randomExtra} to space {spaceIndex}");
        }
    }

    public override float EvaluateOrder(GameObject deliveredPlate)
    {
        List<string> deliveredIngredients = GetIngredientsFromPlate(deliveredPlate);
        
        if (deliveredIngredients.Count == 0)
        {
            Debug.Log("No ingredients found on plate!");
            return 0f;
        }
        
        float score = perfectOrderScore; // Start with perfect score
        
        // Check for perfect match first
        if (IngredientsMatch(deliveredIngredients, requiredIngredients))
        {
            Debug.Log($"Perfect hamburger! Base score: {score}");
            return score; // Early delivery bonus will be added in Customer.cs
        }
        
        // Calculate penalties
        score += CalculateUnwantedIngredientPenalty(deliveredIngredients);
        score += CalculateMissingIngredientPenalty(deliveredIngredients);
        score += CalculateWrongOrderPenalty(deliveredIngredients);
        
        // Ensure score is between 0-100
        score = Mathf.Clamp(score, 0f, 100f);
        
        Debug.Log($"Hamburger evaluation - Required: [{string.Join(", ", requiredIngredients)}]");
        Debug.Log($"Delivered: [{string.Join(", ", deliveredIngredients)}] - Score: {score}");
        
        return score;
    }

    private List<string> GetIngredientsFromPlate(GameObject plate)
    {
        List<string> ingredients = new List<string>();
        
        // Get the container component
        Container container = plate.GetComponent<Container>();
        if (container == null)
        {
            Debug.LogWarning("Delivered plate has no Container component!");
            return ingredients;
        }
        
        // Get all items in the container (bottom to top)
        List<GameObject> items = container.GetAllItems();
        
        foreach (GameObject item in items)
        {
            Ingredient ingredient = item.GetComponent<Ingredient>();
            if (ingredient != null)
            {
                // FIXED: Use ingredient component's name, not GameObject name
                ingredients.Add(ingredient.ingredientName);
            }
            else
            {
                Debug.LogWarning($"Item {item.name} on plate has no Ingredient component!");
            }
        }
        
        return ingredients;
    }
    
    private bool IngredientsMatch(List<string> delivered, List<string> required)
    {
        if (delivered.Count != required.Count) return false;
        
        for (int i = 0; i < delivered.Count; i++)
        {
            if (delivered[i] != required[i]) return false;
        }
        
        return true;
    }
    
    private float CalculateUnwantedIngredientPenalty(List<string> delivered)
    {
        float penalty = 0f;
        List<string> requiredCopy = new List<string>(requiredIngredients);
        
        foreach (string ingredient in delivered)
        {
            if (requiredCopy.Contains(ingredient))
            {
                requiredCopy.Remove(ingredient); // Remove one instance
            }
            else
            {
                penalty += extraIngredientPenalty;
                Debug.Log($"Extra ingredient: {ingredient} (Penalty: {extraIngredientPenalty})");
            }
        }
        
        return penalty;
    }
    
    private float CalculateMissingIngredientPenalty(List<string> delivered)
    {
        float penalty = 0f;
        List<string> deliveredCopy = new List<string>(delivered);
        
        foreach (string ingredient in requiredIngredients)
        {
            if (deliveredCopy.Contains(ingredient))
            {
                deliveredCopy.Remove(ingredient); // Remove one instance
            }
            else
            {
                penalty += missingIngredientPenalty;
                Debug.Log($"Missing ingredient: {ingredient} (Penalty: {missingIngredientPenalty})");
            }
        }
        
        return penalty;
    }

    private float CalculateWrongOrderPenalty(List<string> delivered)
    {
        float penalty = 0f;
        
        // Simple order checking: for each position, check if it matches
        int minLength = Mathf.Min(delivered.Count, requiredIngredients.Count);
        
        for (int i = 0; i < minLength; i++)
        {
            if (delivered[i] != requiredIngredients[i])
            {
                penalty += wrongOrderPenalty;
                Debug.Log($"Wrong order at position {i}: expected {requiredIngredients[i]}, got {delivered[i]} (Penalty: {wrongOrderPenalty})");
            }
        }
        
        return penalty;
    }

    public override float CalculateTotalPrice()
    {
        float total = 0f;
    
        foreach (string ingredientName in requiredIngredients)
        {
            // FIXED: Use database instead of hardcoded values
            total += IngredientDatabaseManager.GetIngredientPrice(ingredientName);
        }
        
        return total;
    }

    public override float CalculateTotalTime()
    {
        float total = 0f;
    
        foreach (string ingredientName in requiredIngredients)
        {
            // FIXED: Use database instead of hardcoded values
            total += IngredientDatabaseManager.GetIngredientTime(ingredientName);
        }
        
        return total;
    }

    public override string GetOrderDescription()
    {
        return $"Hamburger: {string.Join(" → ", requiredIngredients)}";
    }
    
    public override List<string> GetRequiredIngredients()
    {
        return new List<string>(requiredIngredients);
    }

}
