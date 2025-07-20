using System.Collections.Generic;
using UnityEngine;

public class Pan : MonoBehaviour
{
    [Header("Pan Setup")]
    public Transform cookingSpot;  // This becomes the stacking point
    
    private Container container;

    //------------------------------------------------

    void Start()
    {
        container = GetComponent<Container>();
        if (container == null)
        {
            Debug.LogError("Pan requires a Container component!");
            return;
        }
        
        // Subscribe to events for cooking logic integration
        container.OnItemAdded += OnFoodAdded;
        container.OnItemRemoved += OnFoodRemoved;
    }
    
    // Optional wrapper methods
    public bool AddFood(GameObject food)
    {
        return container.TryAddItem(food);
    }
    
    public GameObject RemoveFood()
    {
        return container.TakeTopItem();
    }
    
    public bool HasFood()
    {
        return !container.IsEmpty();
    }
    
    public GameObject GetCurrentFood()
    {
        return container.GetTopItem();
    }
    
    private void OnFoodAdded(GameObject food)
    {
        // Hook for future cooking integration
        Debug.Log($"Food {food.name} added to pan");
    }
    
    private void OnFoodRemoved(GameObject food)
    {
        Debug.Log($"Food {food.name} removed from pan");
    }
}
