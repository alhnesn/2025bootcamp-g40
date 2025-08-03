using System.Collections.Generic;
using UnityEngine;

public class Pan : MonoBehaviour
{
    [Header("Pan Setup")]
    public Transform cookingSpot;  // This becomes the stacking point
    
    private Container container;
   
    //------------------------------------------------
    private CookingSpot currentCookingSpot = null;
    void Start()
    {
        container = GetComponent<Container>();
        if (container == null)
        {
            Debug.LogError("Pan requires a Container component!");
            return;
        }
       
        // // Subscribe to events for cooking logic integration
       
        container.OnItemAdded += OnFoodAdded;
        container.OnItemRemoved += OnFoodRemoved;
    }
    public void SetCurrentCookingSpot(CookingSpot spot)
    {
        currentCookingSpot = spot;
    }
    

   
    
    public bool AddFood(GameObject food) => container.TryAddItem(food);
    public GameObject RemoveFood() => container.TakeTopItem();
    public bool HasFood() => !container.IsEmpty();
    public GameObject GetCurrentFood() => container.GetTopItem();
    
    private void OnFoodAdded(GameObject food)
    {
        // Hook for future cooking integration
        Debug.Log($"Food {food.name} added to pan");
        if (currentCookingSpot != null)
        {
            currentCookingSpot.NotifyFoodAdded();
        }
    }
    
    private void OnFoodRemoved(GameObject food)
    {
        Debug.Log($"Food {food.name} removed from pan");
        if (currentCookingSpot != null)
        {
            currentCookingSpot.NotifyFoodRemoved();
        }
    }
}
