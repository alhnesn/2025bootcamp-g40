// HamburgerOrder.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HamburgerOrder : Order
{
    [Header("Hamburger Specific")]
    public List<string> requiredIngredients = new List<string>(); // Bottom to top
    
    // Available ingredients for hamburger generation
    private static readonly string[] availableIngredients = {
        "BunBottom",
        "BurgerCooked",
        "LettuceSliced", 
        "TomatoSliced",
        "OnionSliced",
        "CheeseSliced",
        "BunTop"
    };
    
    // Mandatory ingredients (always included)
    private static readonly string[] mandatoryIngredients = {
        "BunBottom",
        "BurgerCooked", 
        "BunTop"
    };
    

    public HamburgerOrder()
    {
        orderType = OrderType.Hamburger;
    }
    
    public override void GenerateRandomOrder() // TODO: need real big overhaul for this
    {
        requiredIngredients.Clear();
        
        // Always start with bottom bun
        requiredIngredients.Add("BunBottom");
        
        // Add burger patty (always included)
        requiredIngredients.Add("BurgerCooked");
        
        // Randomly add optional ingredients
        List<string> optionalIngredients = new List<string> { 
            "LettuceSliced", "TomatoSliced", "OnionSliced" 
        };
        
        // Shuffle and pick 1-3 optional ingredients
        optionalIngredients = optionalIngredients.OrderBy(x => Random.Range(0f, 1f)).ToList();
        int ingredientCount = Random.Range(1, 4); // 1 to 3 optional ingredients
        
        for (int i = 0; i < ingredientCount && i < optionalIngredients.Count; i++)
        {
            requiredIngredients.Add(optionalIngredients[i]);
        }
        
        // Always end with top bun
        requiredIngredients.Add("BunTop");
        
        Debug.Log($"Generated hamburger order: {string.Join(" > ", requiredIngredients)}");
    }

    public override float EvaluateOrder(GameObject deliveredPlate)
    {
        // Get ingredients from the delivered plate (bottom to top)
        List<string> deliveredIngredients = GetIngredientsFromPlate(deliveredPlate);
        
        if (deliveredIngredients.Count == 0)
        {
            Debug.Log("No ingredients found on plate!");
            return 0f;
        }
        
        float score = 0f;
        
        // Check for perfect match first
        if (IngredientsMatch(deliveredIngredients, requiredIngredients))
        {
            score = perfectBonus;
            Debug.Log($"Perfect hamburger! Score: {score}");
            return score;
        }
        
        // Calculate penalties
        score += CalculateUnwantedIngredientPenalty(deliveredIngredients);
        score += CalculateMissingIngredientPenalty(deliveredIngredients);
        score += CalculateWrongOrderPenalty(deliveredIngredients);
        
        // Ensure minimum score of 0
        score = Mathf.Max(0f, score);
        
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
                ingredients.Add(ingredient.ingredientName);
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
        
        foreach (string ingredient in delivered)
        {
            if (!requiredIngredients.Contains(ingredient))
            {
                penalty += unwantedIngredientPenalty;
                Debug.Log($"Unwanted ingredient: {ingredient} (Penalty: {unwantedIngredientPenalty})");
            }
        }
        
        return penalty;
    }
    
    private float CalculateMissingIngredientPenalty(List<string> delivered)
    {
        float penalty = 0f;
        
        foreach (string ingredient in requiredIngredients)
        {
            if (!delivered.Contains(ingredient))
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
        
        // Only check order for ingredients that are present in both lists
        List<string> commonIngredients = delivered.Intersect(requiredIngredients).ToList();
        
        // Track positions in both lists
        for (int i = 0; i < commonIngredients.Count; i++)
        {
            string ingredient = commonIngredients[i];
            
            int deliveredIndex = delivered.IndexOf(ingredient);
            int requiredIndex = requiredIngredients.IndexOf(ingredient);
            
            // If ingredient exists in both but in different relative positions
            bool isInWrongOrder = false;
            
            // Check if the relative order is maintained
            for (int j = i + 1; j < commonIngredients.Count; j++)
            {
                string nextIngredient = commonIngredients[j];
                int nextDeliveredIndex = delivered.IndexOf(nextIngredient);
                int nextRequiredIndex = requiredIngredients.IndexOf(nextIngredient);
                
                // If the order relationship is flipped
                if ((deliveredIndex < nextDeliveredIndex) != (requiredIndex < nextRequiredIndex))
                {
                    isInWrongOrder = true;
                    break;
                }
            }
            
            if (isInWrongOrder)
            {
                penalty += wrongOrderPenalty;
                Debug.Log($"Wrong order for ingredient: {ingredient} (Penalty: {wrongOrderPenalty})");
            }
        }
        
        return penalty;
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
